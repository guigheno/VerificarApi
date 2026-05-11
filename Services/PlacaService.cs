using System.Text.Json;
using System.Text.RegularExpressions;
using VerificarApi.Dtos.Placa;
using VerificarApi.Services.Interfaces;

namespace VerificarApi.Services
{
    public class PlacaService : IPlacaService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ILogger<PlacaService> _logger;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public PlacaService(HttpClient httpClient, IConfiguration config, ILogger<PlacaService> logger)
        {
            _httpClient = httpClient;
            _config = config;
            _logger = logger;
        }

        public async Task<ConsultaPlacaResponseDto> ConsultarAsync(string placa)
        {
            // Normaliza a placa: remove espaços, traços e coloca em maiúsculo
            placa = Regex.Replace(placa.Trim().ToUpper(), @"[-\s]", "");

            if (!ValidarFormato(placa))
                throw new ArgumentException($"Formato de placa inválido: '{placa}'. Use AAA9999 ou AAA9A99.");

            var token = _config["ApiPlacas:Token"]
                ?? throw new InvalidOperationException("Token da API de Placas não configurado.");

            var url = $"https://wdapi2.com.br/consulta/{placa}/{token}";
            _logger.LogInformation("Consultando placa {Placa}", placa);

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.GetAsync(url);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erro de conexão com a API de Placas.");
                throw new Exception("Não foi possível conectar à API de Placas. Tente novamente.", ex);
            }

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("API de Placas retornou {Status}: {Body}", response.StatusCode, body);

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    throw new KeyNotFoundException($"Placa '{placa}' não encontrada.");

                throw new Exception($"Erro da API de Placas: {response.StatusCode}");
            }

            var json = await response.Content.ReadAsStringAsync();
            var apiData = JsonSerializer.Deserialize<ApiPlacaResponseDto>(json, JsonOptions)
                ?? throw new Exception("Resposta inválida da API de Placas.");

            _logger.LogInformation("Placa {Placa} consultada: {Marca} {Modelo} {Ano}",
                placa, apiData.Marca, apiData.Modelo, apiData.Ano);

            return MapearResposta(apiData);
        }

        private static ConsultaPlacaResponseDto MapearResposta(ApiPlacaResponseDto api)
        {
            // Restrições: filtra as que são "SEM RESTRICAO"
            var restricoes = new[] {
                api.Extra?.Restricao1,
                api.Extra?.Restricao2,
                api.Extra?.Restricao3,
                api.Extra?.Restricao4
            }
            .Where(r => !string.IsNullOrWhiteSpace(r) &&
                        !r.Equals("SEM RESTRICAO", StringComparison.OrdinalIgnoreCase))
            .Select(r => r!)
            .ToList();

            // Melhor FIPE: maior score
            var melhorFipe = api.Fipe?.Dados
                .OrderByDescending(d => d.Score)
                .FirstOrDefault();

            return new ConsultaPlacaResponseDto
            {
                Placa = api.Placa,
                PlacaAlternativa = api.PlacaAlternativa,
                Marca = api.Marca,
                Modelo = api.Modelo,
                Versao = api.Versao,
                AnoFabricacao = api.Extra?.AnoFabricacao ?? api.Ano,
                AnoModelo = api.Extra?.AnoModelo ?? api.AnoModelo,
                Cor = api.Cor,
                Combustivel = api.Extra?.Combustivel ?? string.Empty,
                Municipio = api.Municipio,
                Uf = api.Extra?.Uf ?? string.Empty,
                Origem = api.Origem,
                Situacao = api.Situacao,
                TipoVeiculo = api.Extra?.TipoVeiculo ?? string.Empty,
                Segmento = api.Segmento,
                Cilindradas = api.Extra?.Cilindradas ?? string.Empty,
                QuantidadePassageiros = api.Extra?.QuantidadePassageiro ?? string.Empty,
                Restricoes = restricoes,
                LogoUrl = api.Logo,
                DataConsulta = api.Data,
                Fipe = melhorFipe == null ? null : new FipeMatchDto
                {
                    CodigoFipe = melhorFipe.CodigoFipe,
                    Modelo = melhorFipe.TextoModelo,
                    Valor = melhorFipe.TextoValor,
                    Combustivel = melhorFipe.Combustivel,
                    AnoModelo = melhorFipe.AnoModelo,
                    MesReferencia = melhorFipe.MesReferencia,
                    Score = melhorFipe.Score
                }
            };
        }

        private static bool ValidarFormato(string placa)
        {
            // Formato antigo: AAA9999
            if (Regex.IsMatch(placa, @"^[A-Z]{3}\d{4}$")) return true;
            // Mercosul: AAA9A99
            if (Regex.IsMatch(placa, @"^[A-Z]{3}\d[A-Z]\d{2}$")) return true;
            return false;
        }
    }
}
