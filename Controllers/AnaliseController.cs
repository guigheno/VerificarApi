using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VerificarApi.Dtos.Analise;
using VerificarApi.Services.Interfaces;

namespace VerificarApi.Controllers
{
    [ApiController]
    [Route("api/analise")]
    [Authorize]
    public class AnaliseController : ControllerBase
    {
        private readonly IFipeService _fipeService;
        private readonly IVehicleAnalysisService _analysisService;
        private readonly IVehicleHistoryService _historyService;
        private readonly IPrecificacaoService _precificacaoService;
        private readonly IUserService _userService;
        private readonly ILogger<AnaliseController> _logger;

        public AnaliseController(
            IFipeService fipeService,
            IVehicleAnalysisService analysisService,
            IVehicleHistoryService historyService,
            IPrecificacaoService precificacaoService,
            IUserService userService,
            ILogger<AnaliseController> logger)
        {
            _fipeService = fipeService;
            _analysisService = analysisService;
            _historyService = historyService;
            _precificacaoService = precificacaoService;
            _userService = userService;
            _logger = logger;
        }

        private Guid UsuarioId =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        /// <summary>
        /// Lista todas as análises de veículos salvas no banco (cache de IA).
        /// Permite filtrar por marca e/ou modelo.
        /// </summary>
        [HttpGet("cache")]
        [ProducesResponseType(typeof(List<VehicleAnalysisDto>), 200)]
        public async Task<ActionResult<List<VehicleAnalysisDto>>> ListarCache(
            [FromQuery] string? marca = null,
            [FromQuery] string? modelo = null,
            [FromQuery] int pagina = 1,
            [FromQuery] int porPagina = 20)
        {
            var analises = await _analysisService.ListarAnalisesSalvasAsync(marca, modelo, pagina, porPagina);
            return Ok(analises);
        }

        /// <summary>
        /// Retorna análise completa do veículo: FIPE + precificação + histórico + análise de IA.
        /// A análise de IA é cacheada no banco — o mesmo modelo/ano nunca é consultado duas vezes.
        /// </summary>
        [HttpGet("completa")]
        [ProducesResponseType(typeof(AnaliseCompletaResponseDto), 200)]
        public async Task<ActionResult<AnaliseCompletaResponseDto>> AnaliseCompleta(
            [FromQuery] string marca,
            [FromQuery] string modelo,
            [FromQuery] int ano,
            [FromQuery] int quilometragem = 0,
            [FromQuery] string condicao = "Não sei",
            [FromQuery] string? placa = null,
            [FromQuery] string combustivel = "Flex",
            [FromQuery] string cambio = "Não sei",
            [FromQuery] string numeroProprietarios = "Não sei",
            [FromQuery] string ipvaEmDia = "Não sei",
            [FromQuery] string revisaoEmDia = "Não sei",
            [FromQuery] string documentacaoRegular = "Não sei",
            [FromQuery] string sinistrado = "Não sei",
            [FromQuery] string estadoPintura = "Não sei",
            [FromQuery] string estadoInterior = "Não sei",
            [FromQuery] string estadoPneus = "Não sei",
            [FromQuery] string estadoRodas = "Não sei")
        {
            if (string.IsNullOrWhiteSpace(marca) || string.IsNullOrWhiteSpace(modelo) || ano < 1985)
                return BadRequest(new { mensagem = "Informe marca, modelo e ano válido." });

            // Consulta por modelo (sem placa) consome 1 crédito de QtdConsultaModelo.
            // A chamada feita após uma consulta por placa envia o parâmetro placa e
            // não consome crédito de modelo — o crédito de placa já foi consumido
            // em /consulta/placa.
            var consultaPorModelo = string.IsNullOrWhiteSpace(placa);
            if (consultaPorModelo)
            {
                var restantes = await _userService.ConsultasRestantesAsync(UsuarioId, TipoConsulta.Modelo);
                if (restantes <= 0)
                    return StatusCode(403, new { mensagem = "Você atingiu o limite de consultas por modelo. Fale com o administrador para liberar mais." });
            }

            try
            {
                _logger.LogInformation("Análise completa: {Marca} {Modelo} {Ano}", marca, modelo, ano);

                var fipe = await _fipeService.ConsultarAsync(marca, modelo, ano);
                var analise = await _analysisService.ObterOuGerarAnaliseAsync(marca, modelo, ano);
                var historico = await _historyService.ListarHistoricoAsync(marca, modelo, ano, placa);

                // Precificação só se tiver valor FIPE e quilometragem informada
                PrecificacaoResponseDto? precificacao = null;
                if (fipe != null && quilometragem > 0)
                {
                    precificacao = _precificacaoService.Calcular(new PrecificacaoRequestDto
                    {
                        Marca = marca,
                        Modelo = modelo,
                        AnoModelo = ano,
                        ValorFipe = fipe.ValorFipeNumerico,
                        Quilometragem = quilometragem,
                        Condicao = condicao,
                        Combustivel = combustivel,
                        Cambio = cambio,
                        NumeroProprietarios = numeroProprietarios,
                        IpvaEmDia = ipvaEmDia,
                        RevisaoEmDia = revisaoEmDia,
                        DocumentacaoRegular = documentacaoRegular,
                        Sinistrado = sinistrado,
                        EstadoPintura = estadoPintura,
                        EstadoInterior = estadoInterior,
                        EstadoPneus = estadoPneus,
                        EstadoRodas = estadoRodas
                    });
                }

                var resultado = new AnaliseCompletaResponseDto
                {
                    Marca = fipe?.Marca ?? marca,
                    Modelo = fipe?.Modelo ?? modelo,
                    AnoModelo = ano,
                    ValorFipe = fipe?.ValorFipe ?? "Não encontrado",
                    ValorFipeNumerico = fipe?.ValorFipeNumerico ?? 0,
                    MesReferencia = fipe?.MesReferencia ?? string.Empty,
                    Precificacao = precificacao,
                    Historico = historico,
                    Analise = analise,
                    PossuiDadosCadastrados = fipe?.DadosComplementares != null
                };

                if (consultaPorModelo)
                    await _userService.ConsumirConsultaAsync(UsuarioId, TipoConsulta.Modelo);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na análise completa para {Marca} {Modelo} {Ano}", marca, modelo, ano);
                return StatusCode(500, new { mensagem = "Erro ao gerar análise. Tente novamente." });
            }
        }

        /// <summary>
        /// Retorna apenas a análise de IA do veículo (com cache).
        /// Consome 1 crédito de consulta por modelo, como a análise completa.
        /// </summary>
        [HttpGet("ia")]
        [ProducesResponseType(typeof(VehicleAnalysisDto), 200)]
        public async Task<ActionResult<VehicleAnalysisDto>> AnaliseIA(
            [FromQuery] string marca,
            [FromQuery] string modelo,
            [FromQuery] int ano)
        {
            if (string.IsNullOrWhiteSpace(marca) || string.IsNullOrWhiteSpace(modelo) || ano < 1985)
                return BadRequest(new { mensagem = "Informe marca, modelo e ano válido." });

            var restantes = await _userService.ConsultasRestantesAsync(UsuarioId, TipoConsulta.Modelo);
            if (restantes <= 0)
                return StatusCode(403, new { mensagem = "Você atingiu o limite de consultas por modelo. Fale com o administrador para liberar mais." });

            try
            {
                var analise = await _analysisService.ObterOuGerarAnaliseAsync(marca, modelo, ano);
                await _userService.ConsumirConsultaAsync(UsuarioId, TipoConsulta.Modelo);
                return Ok(analise);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar análise IA para {Marca} {Modelo} {Ano}", marca, modelo, ano);
                return StatusCode(500, new { mensagem = "Erro ao gerar análise de IA." });
            }
        }

        /// <summary>
        /// Calcula o preço estimado com base no valor FIPE, km e condição.
        /// </summary>
        [HttpPost("precificacao")]
        [ProducesResponseType(typeof(PrecificacaoResponseDto), 200)]
        public ActionResult<PrecificacaoResponseDto> Precificar([FromBody] PrecificacaoRequestDto dto)
        {
            if (dto.ValorFipe <= 0)
                return BadRequest(new { mensagem = "ValorFipe deve ser maior que zero." });

            var resultado = _precificacaoService.Calcular(dto);
            return Ok(resultado);
        }

        /// <summary>
        /// Lista o histórico de eventos de um veículo.
        /// </summary>
        [HttpGet("historico")]
        [ProducesResponseType(typeof(List<VehicleHistoryDto>), 200)]
        public async Task<ActionResult<List<VehicleHistoryDto>>> ListarHistorico(
            [FromQuery] string marca,
            [FromQuery] string modelo,
            [FromQuery] int ano,
            [FromQuery] string? placa = null)
        {
            var historico = await _historyService.ListarHistoricoAsync(marca, modelo, ano, placa);
            return Ok(historico);
        }

        /// <summary>
        /// Adiciona um evento ao histórico de um veículo (Admin).
        /// </summary>
        [HttpPost("historico")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(VehicleHistoryDto), 201)]
        public async Task<ActionResult<VehicleHistoryDto>> AdicionarHistorico([FromBody] CreateHistoryDto dto)
        {
            var evento = await _historyService.AdicionarEventoAsync(dto);
            return CreatedAtAction(nameof(ListarHistorico), new { marca = dto.Marca, modelo = dto.Modelo, ano = dto.AnoModelo }, evento);
        }
    }
}
