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

        public async Task<VehicleAnalysisDto> ObterOuGerarAnaliseAsync(string marca, string modelo, int ano)
        {
            // 1. Tenta buscar no cache
            var marcaNorm = marca.Trim().ToUpper();
            var modeloNorm = modelo.Trim().ToUpper();

            var cache = await _context.VehicleAnalyses
                .Where(a =>
                    a.Marca.ToUpper() == marcaNorm &&
                    a.Modelo.ToUpper() == modeloNorm &&
                    a.AnoModelo == ano)
                .FirstOrDefaultAsync();

            if (cache != null)
            {
                _logger.LogInformation("Análise encontrada no cache para {Marca} {Modelo} {Ano}", marca, modelo, ano);
                return MapearDto(cache, geradoPorIA: false);
            }

            // 2. Não encontrou — gera via OpenAI
            _logger.LogInformation("Gerando análise via IA para {Marca} {Modelo} {Ano}", marca, modelo, ano);
            var analise = await GerarViaOpenAiAsync(marca, modelo, ano);

            // 3. Salva no banco para reaproveitamento
            await _context.VehicleAnalyses.AddAsync(analise);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Análise salva no banco para {Marca} {Modelo} {Ano}", marca, modelo, ano);
            return MapearDto(analise, geradoPorIA: true);
        }

        private async Task<VehicleAnalysis> GerarViaOpenAiAsync(string marca, string modelo, int ano)
        {
            var apiKey = _config["OpenAI:ApiKey"]
                ?? throw new InvalidOperationException("Chave da OpenAI não configurada.");

            var prompt = "Você é um especialista em veículos usados no mercado brasileiro. " +
                $"Analise o veículo: {marca} {modelo} {ano}. " +
                "Responda APENAS com um JSON válido, sem markdown, sem explicações, exatamente neste formato: " +
                "{\"problemas_cronicos\": \"lista dos principais problemas mecânicos/elétricos conhecidos deste modelo/ano, separados por ponto e vírgula\"," +
                "\"pontos_positivos\": \"principais vantagens do veículo, separadas por ponto e vírgula\"," +
                "\"pontos_negativos\": \"principais desvantagens do veículo, separadas por ponto e vírgula\"," +
                "\"resumo_geral\": \"parágrafo de 2 a 3 frases resumindo o veículo para um comprador\"," +
                "\"nota_confiabilidade\": 3.5," +
                "\"nota_custo_manutencao\": 4.0," +
                "\"nota_conforto\": 3.8," +
                "\"nota_desempenho\": 3.2," +
                "\"nota_consumo\": 3.9," +
                "\"nota_espaco_interno\": 3.5} " +
                "As notas devem ser de 0.0 a 5.0. Use uma casa decimal. Seja objetivo e baseado em dados reais do mercado brasileiro.";

            var requestBody = new
            {
                model = ModeloIA,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                temperature = 0.3,
                max_tokens = 800
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

            // Parse do JSON retornado pela IA
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
                GeradoEm = a.GeradoEm
            };
        }
    }
}
