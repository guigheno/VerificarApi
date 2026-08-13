using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VerificarApi.Dtos.RegistrationLinks;
using VerificarApi.Services.Interfaces;

namespace VerificarApi.Controllers
{
    [ApiController]
    [Route("api/registration-links")]
    public class RegistrationLinksController : ControllerBase
    {
        private readonly IRegistrationLinkService _linkService;

        public RegistrationLinksController(IRegistrationLinkService linkService)
        {
            _linkService = linkService;
        }

        /// <summary>
        /// Valida um link de registro (público) — usado pela tela de cadastro
        /// para confirmar que o link é válido antes de exibir o formulário.
        /// </summary>
        [HttpGet("validar/{token}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ValidarRegistrationLinkDto), 200)]
        public async Task<ActionResult<ValidarRegistrationLinkDto>> Validar(string token)
        {
            return Ok(await _linkService.ValidarAsync(token));
        }

        /// <summary>
        /// Lista todos os links de registro (Admin).
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(List<RegistrationLinkResponseDto>), 200)]
        public async Task<ActionResult<List<RegistrationLinkResponseDto>>> Listar()
        {
            return Ok(await _linkService.ListarAsync());
        }

        /// <summary>
        /// Cria um novo link de registro (Admin).
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(RegistrationLinkResponseDto), 201)]
        public async Task<ActionResult<RegistrationLinkResponseDto>> Criar(CreateRegistrationLinkDto dto)
        {
            var link = await _linkService.CriarAsync(dto);
            return CreatedAtAction(nameof(Listar), new { }, link);
        }

        /// <summary>
        /// Atualiza um link de registro (Admin).
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(RegistrationLinkResponseDto), 200)]
        public async Task<ActionResult<RegistrationLinkResponseDto>> Atualizar(Guid id, UpdateRegistrationLinkDto dto)
        {
            var link = await _linkService.AtualizarAsync(id, dto);

            if (link == null)
                return NotFound();

            return Ok(link);
        }

        /// <summary>
        /// Exclui um link de registro (Admin).
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Excluir(Guid id)
        {
            var excluido = await _linkService.ExcluirAsync(id);

            if (!excluido)
                return NotFound();

            return NoContent();
        }
    }
}
