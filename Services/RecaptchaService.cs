using System.Net.Http.Json;
using System.Text.Json.Serialization;
using VerificarApi.Services.Interfaces;

namespace VerificarApi.Services
{
    public class RecaptchaService : IRecaptchaService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ILogger<RecaptchaService> _logger;

        private const string SiteVerifyUrl = "https://www.google.com/recaptcha/api/siteverify";

        public RecaptchaService(HttpClient httpClient, IConfiguration config, ILogger<RecaptchaService> logger)
        {
            _httpClient = httpClient;
            _config = config;
            _logger = logger;
        }

        public async Task<bool> VerificarAsync(string? token, string? remoteIp, string action)
        {
            var habilitado = _config.GetValue("Recaptcha:Enabled", true);
            if (!habilitado)
                return true;

            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("reCAPTCHA: token ausente para a action {Action}.", action);
                return false;
            }

            var secret = _config["Recaptcha:SecretKey"];
            if (string.IsNullOrWhiteSpace(secret))
            {
                _logger.LogWarning("reCAPTCHA: SecretKey não configurada. Bloqueando registro.");
                return false;
            }

            try
            {
                using var form = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["secret"] = secret,
                    ["response"] = token,
                    ["remoteip"] = remoteIp ?? string.Empty
                });

                var response = await _httpClient.PostAsync(SiteVerifyUrl, form);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("reCAPTCHA: siteverify retornou {Status}.", response.StatusCode);
                    return false;
                }

                var result = await response.Content.ReadFromJsonAsync<RecaptchaResponse>();
                if (result == null || !result.Success)
                {
                    var erros = result == null ? "resposta nula" : string.Join(", ", result.ErrorCodes ?? Array.Empty<string>());
                    _logger.LogWarning("reCAPTCHA: validação falhou ({Erros}).", erros);
                    return false;
                }

                var minScore = _config.GetValue("Recaptcha:MinScore", 0.5);
                if (result.Score < minScore)
                {
                    _logger.LogWarning("reCAPTCHA: score {Score} abaixo do mínimo {Min} (action {Action}, hostname {Hostname}).",
                        result.Score, minScore, result.Action, result.Hostname);
                    return false;
                }

                if (!string.Equals(result.Action, action, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("reCAPTCHA: action {Esperada} recebida {Recebida}.", action, result.Action);
                    return false;
                }

                var permitidos = _config.GetSection("Recaptcha:AllowedHostnames").Get<string[]>()
                    ?? new[] { "verificar.site", "www.verificar.site", "localhost" };

                if (!permitidos.Contains(result.Hostname, StringComparer.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("reCAPTCHA: hostname {Hostname} não permitido.", result.Hostname);
                    return false;
                }

                _logger.LogInformation("reCAPTCHA: validado com sucesso (score {Score}, action {Action}).", result.Score, result.Action);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "reCAPTCHA: erro ao consultar o Google.");
                return false;
            }
        }

        private sealed class RecaptchaResponse
        {
            [JsonPropertyName("success")]
            public bool Success { get; set; }

            [JsonPropertyName("score")]
            public double Score { get; set; }

            [JsonPropertyName("action")]
            public string? Action { get; set; }

            [JsonPropertyName("hostname")]
            public string? Hostname { get; set; }

            [JsonPropertyName("challenge_ts")]
            public string? ChallengeTs { get; set; }

            [JsonPropertyName("error-codes")]
            public string[]? ErrorCodes { get; set; }
        }
    }
}
