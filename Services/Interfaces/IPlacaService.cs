using VerificarApi.Dtos.Placa;

namespace VerificarApi.Services.Interfaces
{
    public interface IPlacaService
    {
        /// <summary>
        /// Consulta os dados de um veículo pela placa usando a API de placas.
        /// Aceita tanto o formato antigo (AAA9999) quanto o Mercosul (AAA9A99).
        /// </summary>
        Task<ConsultaPlacaResponseDto> ConsultarAsync(string placa);
    }
}
