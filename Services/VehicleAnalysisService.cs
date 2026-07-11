using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using VerificarApi.Domain.Entities;
using VerificarApi.Dtos.Analise;
using VerificarApi.Infrastructure.Data;
using VerificarApi.Services.Interfaces;

namespace VerificarApi.Services
{
    public class VehicleAnalysisService : IVehicleAnalysisService
    {
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ILogger<VehicleAnalysisService> _logger;

        private const string ModeloIA = "gpt-4o-mini";

        public VehicleAnalysisService(
            AppDbContext context,
            HttpClient httpClient,
            IConfiguration config,
            ILogger<VehicleAnalysisService> logger)
        {
            _context = context;
            _httpClient = httpClient;
            _config = config;
            _logger = logger;
        }

        /// <summary>
        /// Calcula a validade da análise:
        /// 10% da idade do carro em meses, mínimo de 1 mês.
        /// Ex: carro de 10 anos = 12 meses de validade; carro de 2 anos = 2,4 meses (~73 dias); carro novo = 1 mês.
        /// </summary>
        private static DateTime CalcularProximaAtualizacao(int anoModelo)
        {
            int anoAtual = DateTime.UtcNow.Year;
            int idadeAnos = Math.Max(anoAtual - anoModelo, 0);
            double validadeMeses = Math.Max(idadeAnos * 12 * 0.10, 1.0); // 10% da idade em meses, mínimo 1
            return DateTime.UtcNow.AddMonths((int)Math.Floor(validadeMeses))
                                  .AddDays((validadeMeses % 1) * 30); // fração em dias
        }

        public async Task<List<VehicleAnalysisDto>> ListarAnalisesSalvasAsync(string? marca, string? modelo, int pagina, int porPagina)
        {
            var query = _context.VehicleAnalyses.AsQueryable();

            if (!string.IsNullOrWhiteSpace(marca))
                query = query.Where(a => a.Marca.ToUpper().Contains(marca.ToUpper()));

            if (!string.IsNullOrWhiteSpace(modelo))
                query = query.Where(a => a.Modelo.ToUpper().Contains(modelo.ToUpper()));

            var analises = await query
                .OrderBy(a => a.Marca)
                .ThenBy(a => a.Modelo)
                .ThenBy(a => a.AnoModelo)
                .Skip((pagina - 1) * porPagina)
                .Take(porPagina)
                .ToListAsync();

            return analises.Select(a => MapearDto(a, geradoPorIA: false)).ToList();
        }

        public async Task<VehicleAnalysisDto> ObterOuGerarAnaliseAsync(string marca, string modelo, int ano)
        {
            var marcaNorm = marca.Trim().ToUpper();
            var modeloNorm = modelo.Trim().ToUpper();
            var agora = DateTime.UtcNow;

            var cache = await _context.VehicleAnalyses
                .Where(a =>
                    a.Marca.ToUpper() == marcaNorm &&
                    a.Modelo.ToUpper() == modeloNorm &&
                    a.AnoModelo == ano)
                .FirstOrDefaultAsync();

            // Cache existe e ainda é válido
            if (cache != null && agora < cache.ProximaAtualizacaoEm)
            {
                _logger.LogInformation(
                    "Cache válido para {Marca} {Modelo} {Ano}. Válido até {Validade}",
                    marca, modelo, ano, cache.ProximaAtualizacaoEm.ToString("dd/MM/yyyy"));

                // Atualiza só o timestamp de consulta
                cache.UltimaConsultaEm = agora;
                await _context.SaveChangesAsync();

                return MapearDto(cache, geradoPorIA: false);
            }

            // Cache existe mas está vencido — regenera
            if (cache != null && agora >= cache.ProximaAtualizacaoEm)
            {
                _logger.LogInformation(
                    "Cache vencido para {Marca} {Modelo} {Ano} (venceu em {Vencimento}). Regenerando...",
                    marca, modelo, ano, cache.ProximaAtualizacaoEm.ToString("dd/MM/yyyy"));

                var analiseAtualizada = await GerarViaOpenAiAsync(marca, modelo, ano);

                // Atualiza os campos existentes
                cache.ProblemasCronicos = analiseAtualizada.ProblemasCronicos;
                cache.PontosPositivos = analiseAtualizada.PontosPositivos;
                cache.PontosNegativos = analiseAtualizada.PontosNegativos;
                cache.ResumoGeral = analiseAtualizada.ResumoGeral;
                cache.NotaConfiabilidade = analiseAtualizada.NotaConfiabilidade;
                cache.NotaCustoManutencao = analiseAtualizada.NotaCustoManutencao;
                cache.NotaConforto = analiseAtualizada.NotaConforto;
                cache.NotaDesempenho = analiseAtualizada.NotaDesempenho;
                cache.NotaConsumo = analiseAtualizada.NotaConsumo;
                cache.NotaEspacoInterno = analiseAtualizada.NotaEspacoInterno;
                cache.GeradoEm = agora;
                cache.UltimaConsultaEm = agora;
                cache.ProximaAtualizacaoEm = analiseAtualizada.ProximaAtualizacaoEm;
                cache.ModeloIA = analiseAtualizada.ModeloIA;

                await _context.SaveChangesAsync();
                return MapearDto(cache, geradoPorIA: true);
            }

            // Não existe cache — gera pela primeira vez
            _logger.LogInformation("Gerando análise via IA para {Marca} {Modelo} {Ano}", marca, modelo, ano);
            var novaAnalise = await GerarViaOpenAiAsync(marca, modelo, ano);

            await _context.VehicleAnalyses.AddAsync(novaAnalise);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Análise salva. Próxima atualização: {ProximaAtualizacao}",
                novaAnalise.ProximaAtualizacaoEm.ToString("dd/MM/yyyy"));

            return MapearDto(novaAnalise, geradoPorIA: true);
        }

        private async Task<VehicleAnalysis> GerarViaOpenAiAsync(string marca, string modelo, int ano)
        {
            var apiKey = _config["OpenAI:ApiKey"]
                ?? throw new InvalidOperationException("Chave da OpenAI não configurada.");

            var prompt =
                "Você é um especialista em veículos usados no mercado brasileiro com acesso a relatos de fóruns, recalls, " +
                "reclamações no Reclame Aqui e laudos técnicos. " +
                $"Analise o veículo: {marca} {modelo} {ano}. " +
                "Responda APENAS com um JSON válido, sem markdown, sem explicações. Use exatamente este formato:\n" +
                "{" +
                "\"problemas_cronicos\": \"SOMENTE defeitos crônicos REAIS, amplamente documentados e recorrentes neste modelo/geração específica. " +
                "Inclua apenas falhas que aparecem com frequência significativa entre proprietários (ex: desgaste precoce de correia, falha no módulo de injeção). " +
                "Separe por ponto e vírgula. Se este modelo NÃO possui defeitos crônicos documentados relevantes, retorne string vazia.\"," +
                "\"pontos_positivos\": \"principais vantagens comprovadas do veículo para o mercado brasileiro, separadas por ponto e vírgula\"," +
                "\"pontos_negativos\": \"principais desvantagens reais do veículo (excluindo defeitos crônicos já listados), separadas por ponto e vírgula\"," +
                "\"resumo_geral\": \"parágrafo de 2 a 3 frases objetivo e neutro resumindo este veículo para um comprador que avalia a compra\"," +
                "\"nota_confiabilidade\": 0.0," +
                "\"nota_custo_manutencao\": 0.0," +
                "\"nota_conforto\": 0.0," +
                "\"nota_desempenho\": 0.0," +
                "\"nota_consumo\": 0.0," +
                "\"nota_espaco_interno\": 0.0" +
                "} " +
                "Substitua os 0.0 pelas notas reais de 0.0 a 5.0 com uma casa decimal, baseadas em dados reais e comparativos do mercado brasileiro. " +
                "5.0 = excelente, 3.0 = mediano, 1.0 = ruim.";
                
            var requestBody = new
            {
                model = ModeloIA,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                temperature = 0.3,
                max_tokens = 1500
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);

            if (!response.IsSuccessStatusCode)
            {
                var erro = await response.Content.ReadAsStringAsync();
                _logger.LogError("OpenAI retornou erro {Status}: {Erro}", response.StatusCode, erro);
                throw new Exception($"Erro na API OpenAI: {response.StatusCode}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);
            var textoResposta = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "{}";

            using var analiseDoc = JsonDocument.Parse(textoResposta.Trim());
            var root = analiseDoc.RootElement;

            return new VehicleAnalysis
            {
                Id = Guid.NewGuid(),
                Marca = marca,
                Modelo = modelo,
                AnoModelo = ano,
                ProblemasCronicos = root.GetProperty("problemas_cronicos").GetString() ?? string.Empty,
                PontosPositivos = root.GetProperty("pontos_positivos").GetString() ?? string.Empty,
                PontosNegativos = root.GetProperty("pontos_negativos").GetString() ?? string.Empty,
                ResumoGeral = root.GetProperty("resumo_geral").GetString() ?? string.Empty,
                NotaConfiabilidade = root.GetProperty("nota_confiabilidade").GetDecimal(),
                NotaCustoManutencao = root.GetProperty("nota_custo_manutencao").GetDecimal(),
                NotaConforto = root.GetProperty("nota_conforto").GetDecimal(),
                NotaDesempenho = root.GetProperty("nota_desempenho").GetDecimal(),
                NotaConsumo = root.GetProperty("nota_consumo").GetDecimal(),
                NotaEspacoInterno = root.GetProperty("nota_espaco_interno").GetDecimal(),
                GeradoEm = DateTime.UtcNow,
                UltimaConsultaEm = DateTime.UtcNow,
                ProximaAtualizacaoEm = CalcularProximaAtualizacao(ano),
                ModeloIA = ModeloIA
            };
        }

        private static VehicleAnalysisDto MapearDto(VehicleAnalysis a, bool geradoPorIA)
        {
            var notaGeral = (a.NotaConfiabilidade + a.NotaCustoManutencao + a.NotaConforto +
                            a.NotaDesempenho + a.NotaConsumo + a.NotaEspacoInterno) / 6;

            return new VehicleAnalysisDto
            {
                Marca = a.Marca,
                Modelo = a.Modelo,
                AnoModelo = a.AnoModelo,
                ProblemasCronicos = a.ProblemasCronicos,
                PontosPositivos = a.PontosPositivos,
                PontosNegativos = a.PontosNegativos,
                ResumoGeral = a.ResumoGeral,
                NotaConfiabilidade = a.NotaConfiabilidade,
                NotaCustoManutencao = a.NotaCustoManutencao,
                NotaConforto = a.NotaConforto,
                NotaDesempenho = a.NotaDesempenho,
                NotaConsumo = a.NotaConsumo,
                NotaEspacoInterno = a.NotaEspacoInterno,
                NotaGeral = Math.Round(notaGeral, 1),
                GeradoPorIA = geradoPorIA,
                GeradoEm = a.GeradoEm,
                UltimaConsultaEm = a.UltimaConsultaEm,
                ProximaAtualizacaoEm = a.ProximaAtualizacaoEm
            };
        }
    }
}
