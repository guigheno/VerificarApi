namespace VerificarApi.Services.Interfaces
{
    public interface IRecaptchaService
    {
        /// <summary>
        /// Valida um token do Google reCAPTCHA v3 junto ao Google (siteverify).
        /// Retorna true quando: config habilitada e token válido com score >= MinScore
        /// para a action esperada; quando a config está desabilitada, retorna true (passa).
        /// </summary>
        Task<bool> VerificarAsync(string? token, string? remoteIp, string action);
    }
}
