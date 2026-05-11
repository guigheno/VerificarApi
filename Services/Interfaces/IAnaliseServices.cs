using VerificarApi.Dtos.Analise;

namespace VerificarApi.Services.Interfaces
{
    public interface IPrecificacaoService
    {
        PrecificacaoResponseDto Calcular(PrecificacaoRequestDto request);
    }

    public interface IVehicleAnalysisService
    {
        /// <summary>
        /// Retorna análise do veículo. Se já existir no banco, reutiliza.
        /// Se não, gera via OpenAI e salva para reaproveitamento.
        /// </summary>
        Task<VehicleAnalysisDto> ObterOuGerarAnaliseAsync(string marca, string modelo, int ano);
    }

    public interface IVehicleHistoryService
    {
        Task<List<VehicleHistoryDto>> ListarHistoricoAsync(string marca, string modelo, int ano, string? placa = null);
        Task<VehicleHistoryDto> AdicionarEventoAsync(CreateHistoryDto dto);
    }
}
