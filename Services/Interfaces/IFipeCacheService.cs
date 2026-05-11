using VerificarApi.Dtos.Fipe;

namespace VerificarApi.Services.Interfaces
{
    public interface IFipeCacheService
    {
        /// <summary>
        /// Sincroniza todas as marcas e modelos da API FIPE para o banco local.
        /// Apaga os dados antigos e reinsere tudo. Pode demorar alguns minutos.
        /// </summary>
        Task<FipeSyncResultDto> SincronizarAsync();

        /// <summary>
        /// Retorna todas as marcas do cache local.
        /// </summary>
        Task<List<FipeMarcaDto>> ListarMarcasAsync();

        /// <summary>
        /// Retorna os modelos de uma marca pelo código FIPE da marca.
        /// </summary>
        Task<List<FipeModeloDto>> ListarModelosPorMarcaAsync(string codigoMarca);

        /// <summary>
        /// Verifica se o cache já foi populado.
        /// </summary>
        Task<bool> CachePopuladoAsync();
    }
}
