using VerificarApi.Dtos.Fipe;

namespace VerificarApi.Services.Interfaces
{
    public interface IFipeService
    {
        /// <summary>
        /// Consulta o valor FIPE de um veículo por marca, modelo e ano,
        /// e enriquece com dados complementares do nosso banco se disponíveis.
        /// </summary>
        Task<ConsultaFipeResponseDto?> ConsultarAsync(string marca, string modelo, int ano);

        /// <summary>
        /// Lista todas as marcas disponíveis na FIPE (tipo 1 = carros).
        /// </summary>
        Task<List<FipeMarcaDto>> ListarMarcasAsync();

        /// <summary>
        /// Lista os anos disponíveis de um modelo na FIPE.
        /// </summary>
        Task<List<FipeAnoDto>> ListarAnosAsync(string codigoMarca, int codigoModelo);
    }
}
