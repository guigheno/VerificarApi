using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VerificarApi.Dtos.Fipe;
using VerificarApi.Services.Interfaces;

namespace VerificarApi.Controllers
{
    [ApiController]
    [Route("api/consulta")]
    [Authorize]
    public class ConsultaController : ControllerBase
    {
        private readonly IFipeService _fipeService;
        private readonly IFipeCacheService _fipeCacheService;
        private readonly ILogger<ConsultaController> _logger;

        public ConsultaController(
            IFipeService fipeService,
            IFipeCacheService fipeCacheService,
            ILogger<ConsultaController> logger)
        {
            _fipeService = fipeService;
            _fipeCacheService = fipeCacheService;
            _logger = logger;
        }

        /// <summary>
        /// Retorna todas as marcas disponíveis (leitura do cache local).
        /// </summary>
        [HttpGet("marcas")]
        [ProducesResponseType(typeof(List<FipeMarcaDto>), 200)]
        public async Task<ActionResult<List<FipeMarcaDto>>> ListarMarcas()
        {
            var marcas = await _fipeCacheService.ListarMarcasAsync();

            if (!marcas.Any())
                return NotFound(new { mensagem = "Cache FIPE vazio. Solicite ao administrador que execute o sync." });

            return Ok(marcas);
        }

        /// <summary>
        /// Retorna os modelos de uma marca pelo código FIPE da marca (leitura do cache local).
        /// </summary>
        [HttpGet("marcas/{codigoMarca}/modelos")]
        [ProducesResponseType(typeof(List<FipeModeloDto>), 200)]
        public async Task<ActionResult<List<FipeModeloDto>>> ListarModelos(string codigoMarca)
        {
            var modelos = await _fipeCacheService.ListarModelosPorMarcaAsync(codigoMarca);

            if (!modelos.Any())
                return NotFound(new { mensagem = $"Nenhum modelo encontrado para o código de marca '{codigoMarca}'." });

            return Ok(modelos);
        }

        /// <summary>
        /// Retorna os anos disponíveis de um modelo diretamente da API FIPE.
        /// </summary>
        [HttpGet("marcas/{codigoMarca}/modelos/{codigoModelo}/anos")]
        [ProducesResponseType(typeof(List<FipeAnoDto>), 200)]
        public async Task<ActionResult<List<FipeAnoDto>>> ListarAnos(string codigoMarca, int codigoModelo)
        {
            try
            {
                var anos = await _fipeService.ListarAnosAsync(codigoMarca, codigoModelo);

                if (!anos.Any())
                    return NotFound(new { mensagem = "Nenhum ano encontrado para este modelo." });

                return Ok(anos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar anos para marca {Marca} modelo {Modelo}", codigoMarca, codigoModelo);
                return StatusCode(502, new { mensagem = "Erro ao consultar a API FIPE." });
            }
        }

        /// <summary>
        /// Consulta o valor FIPE atual e dados complementares para um veículo.
        /// Só aqui é feita a chamada à API FIPE — após o usuário selecionar marca, modelo e ano.
        /// </summary>
        [HttpGet("fipe")]
        [ProducesResponseType(typeof(ConsultaFipeResponseDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(502)]
        public async Task<ActionResult<ConsultaFipeResponseDto>> ConsultarFipe(
            [FromQuery] string marca,
            [FromQuery] string modelo,
            [FromQuery] int ano)
        {
            if (string.IsNullOrWhiteSpace(marca) || string.IsNullOrWhiteSpace(modelo) || ano < 1985)
                return BadRequest(new { mensagem = "Informe marca, modelo e ano válido (mínimo 1985)." });

            try
            {
                _logger.LogInformation("Consulta FIPE: {Marca} {Modelo} {Ano}", marca, modelo, ano);

                var resultado = await _fipeService.ConsultarAsync(marca, modelo, ano);

                if (resultado == null)
                    return NotFound(new { mensagem = $"Veículo '{marca} {modelo} {ano}' não encontrado na tabela FIPE." });

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na consulta FIPE para {Marca} {Modelo} {Ano}", marca, modelo, ano);
                return StatusCode(502, new { mensagem = "Erro ao consultar a API FIPE. Tente novamente." });
            }
        }
    }
}
