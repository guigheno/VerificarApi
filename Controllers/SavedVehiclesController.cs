using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VerificarApi.Dtos.SavedVehicles;
using VerificarApi.Services.Interfaces;

namespace VerificarApi.Controllers
{
    [ApiController]
    [Route("api/veiculos-salvos")]
    [Authorize]
    public class SavedVehiclesController : ControllerBase
    {
        private readonly ISavedVehicleService _savedVehicleService;

        public SavedVehiclesController(ISavedVehicleService savedVehicleService)
        {
            _savedVehicleService = savedVehicleService;
        }

        private Guid UsuarioId =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        /// <summary>
        /// Lista os veículos salvos no perfil do usuário logado.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<SavedVehicleResponseDto>), 200)]
        public async Task<ActionResult<List<SavedVehicleResponseDto>>> Listar()
        {
            var veiculos = await _savedVehicleService.ListarAsync(UsuarioId);
            return Ok(veiculos);
        }

        /// <summary>
        /// Salva um veículo consultado no perfil do usuário (ou atualiza o snapshot se já estiver salvo).
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(SavedVehicleResponseDto), 200)]
        public async Task<ActionResult<SavedVehicleResponseDto>> Salvar([FromBody] CreateSavedVehicleDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Marca) || string.IsNullOrWhiteSpace(dto.Modelo) || dto.AnoModelo < 1985)
                return BadRequest(new { mensagem = "Informe marca, modelo e ano válido." });

            if (dto.ValorFipe <= 0)
                return BadRequest(new { mensagem = "Não é possível salvar um veículo sem valor FIPE encontrado." });

            var salvo = await _savedVehicleService.SalvarAsync(UsuarioId, dto);
            return Ok(salvo);
        }

        /// <summary>
        /// Remove um veículo salvo do perfil do usuário logado.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Remover(Guid id)
        {
            var removido = await _savedVehicleService.RemoverAsync(UsuarioId, id);
            if (!removido)
                return NotFound();

            return NoContent();
        }
    }
}
