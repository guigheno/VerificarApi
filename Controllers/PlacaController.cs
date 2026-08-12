using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VerificarApi.Dtos.Placa;
using VerificarApi.Services.Interfaces;

namespace VerificarApi.Controllers
{
    [ApiController]
    [Route("api/consulta/placa")]
    [Authorize]
    public class PlacaController : ControllerBase
    {
        private readonly IPlacaService _placaService;
        private readonly IUserService _userService;
        private readonly ILogger<PlacaController> _logger;

        public PlacaController(IPlacaService placaService, IUserService userService, ILogger<PlacaController> logger)
        {
            _placaService = placaService;
            _userService = userService;
            _logger = logger;
        }

        private Guid UsuarioId =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        /// <summary>
        /// Consulta os dados de um veículo pela placa.
        /// Aceita formato antigo (AAA9999) e Mercosul (AAA9A99).
        /// </summary>
        /// <param name="placa">Placa do veículo. Ex: IUF9E48 ou IUF9448</param>
        [HttpGet("{placa}")]
        [ProducesResponseType(typeof(ConsultaPlacaResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(502)]
        public async Task<ActionResult<ConsultaPlacaResponseDto>> ConsultarPlaca(string placa)
        {
            var restantes = await _userService.ConsultasRestantesAsync(UsuarioId, TipoConsulta.Placa);
            if (restantes <= 0)
                return StatusCode(403, new { mensagem = "Você atingiu o limite de consultas por placa. Fale com o administrador para liberar mais." });

            try
            {
                var resultado = await _placaService.ConsultarAsync(placa);

                await _userService.ConsumirConsultaAsync(UsuarioId, TipoConsulta.Placa);

                return Ok(resultado);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensagem = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consultar placa {Placa}", placa);
                return StatusCode(502, new { mensagem = "Erro ao consultar a API de Placas. Tente novamente." });
            }
        }
    }
}
