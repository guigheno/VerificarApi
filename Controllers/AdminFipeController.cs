using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VerificarApi.Dtos.Fipe;
using VerificarApi.Services.Interfaces;

namespace VerificarApi.Controllers
{
    [ApiController]
    [Route("api/admin/fipe")]
    [Authorize(Roles = "Admin")]
    public class AdminFipeController : ControllerBase
    {
        private readonly IFipeCacheService _fipeCacheService;
        private readonly ILogger<AdminFipeController> _logger;

        public AdminFipeController(IFipeCacheService fipeCacheService, ILogger<AdminFipeController> logger)
        {
            _fipeCacheService = fipeCacheService;
            _logger = logger;
        }

        /// <summary>
        /// Sincroniza todas as marcas e modelos da API FIPE para o banco local.
        /// Atenção: pode demorar alguns minutos dependendo da quantidade de marcas.
        /// </summary>
        [HttpPost("sync")]
        [ProducesResponseType(typeof(FipeSyncResultDto), 200)]
        public async Task<ActionResult<FipeSyncResultDto>> Sincronizar()
        {
            _logger.LogInformation("Sync FIPE iniciado pelo admin {User}.", User.Identity?.Name);

            var resultado = await _fipeCacheService.SincronizarAsync();
            return Ok(resultado);
        }

        /// <summary>
        /// Verifica o status do cache FIPE (se já foi populado).
        /// </summary>
        [HttpGet("status")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Status()
        {
            var populado = await _fipeCacheService.CachePopuladoAsync();
            return Ok(new
            {
                cachePopulado = populado,
                mensagem = populado
                    ? "Cache FIPE está ativo. Use POST /sync para atualizar."
                    : "Cache FIPE está vazio. Execute POST /sync para popular."
            });
        }
    }
}
