using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;
using VerificarApi.Dtos.Fipe;
using VerificarApi.Infrastructure.Data;
using VerificarApi.Services.Interfaces;

namespace VerificarApi.Services
{
    public class FipeService : IFipeService
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _context;
        private readonly ILogger<FipeService> _logger;

        private const string BaseUrl = "https://parallelum.com.br/fipe/api/v1/carros";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public FipeService(HttpClient httpClient, AppDbContext context, ILogger<FipeService> logger)
        {
            _httpClient = httpClient;
            _context = context;
            _logger = logger;
        }

        public async Task<List<FipeMarcaDto>> ListarMarcasAsync()
        {
            var url = $"{BaseUrl}/marcas";
            _logger.LogInformation("Consultando marcas FIPE: {Url}", url);

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<FipeMarcaDto>>(json, JsonOptions) ?? new();
        }

        public async Task<List<FipeAnoDto>> ListarAnosAsync(string codigoMarca, int codigoModelo)
        {
            var url = $"{BaseUrl}/marcas/{codigoMarca}/modelos/{codigoModelo}/anos";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<FipeAnoDto>>(json, JsonOptions) ?? new();
        }

        public async Task<List<FipeModeloDto>> ListarModelosAsync(string marcaNome)
        {
            var marcas = await ListarMarcasAsync();
            var marca = EncontrarMelhorCorrespondencia(marcas, marcaNome, m => m.Nome);

            if (marca == null)
            {
                _logger.LogWarning("Marca não encontrada na FIPE: {Marca}", marcaNome);
                return new();
            }

            var url = $"{BaseUrl}/marcas/{marca.Codigo}/modelos";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var resultado = JsonSerializer.Deserialize<FipeModelosResponseDto>(json, JsonOptions);
            return resultado?.Modelos ?? new();
        }

        public async Task<ConsultaFipeResponseDto?> ConsultarAsync(string marcaNome, string modeloNome, int ano)
        {
            try
            {
                // 1. Busca marcas e encontra a correta
                var marcas = await ListarMarcasAsync();
                var marca = EncontrarMelhorCorrespondencia(marcas, marcaNome, m => m.Nome);
                if (marca == null)
                {
                    _logger.LogWarning("Marca '{Marca}' não encontrada na FIPE.", marcaNome);
                    return null;
                }

                // 2. Busca modelos da marca
                var modelosUrl = $"{BaseUrl}/marcas/{marca.Codigo}/modelos";
                var modelosResponse = await _httpClient.GetAsync(modelosUrl);
                modelosResponse.EnsureSuccessStatusCode();

                var modelosJson = await modelosResponse.Content.ReadAsStringAsync();
                var modelosData = JsonSerializer.Deserialize<FipeModelosResponseDto>(modelosJson, JsonOptions);
                if (modelosData == null) return null;

                var modelo = EncontrarMelhorCorrespondencia(modelosData.Modelos, modeloNome, m => m.Nome);
                if (modelo == null)
                {
                    _logger.LogWarning("Modelo '{Modelo}' não encontrado para a marca '{Marca}'.", modeloNome, marcaNome);
                    return null;
                }

                // 3. Busca anos disponíveis para o modelo
                var anosUrl = $"{BaseUrl}/marcas/{marca.Codigo}/modelos/{modelo.Codigo}/anos";
                var anosResponse = await _httpClient.GetAsync(anosUrl);
                anosResponse.EnsureSuccessStatusCode();

                var anosJson = await anosResponse.Content.ReadAsStringAsync();
                var anos = JsonSerializer.Deserialize<List<FipeAnoDto>>(anosJson, JsonOptions) ?? new();

                // Tenta encontrar o ano exato, senão pega o mais próximo
                var anoEncontrado = anos.FirstOrDefault(a => a.Nome.StartsWith(ano.ToString()))
                                ?? anos.OrderByDescending(a => a.Nome).FirstOrDefault();

                if (anoEncontrado == null)
                {
                    _logger.LogWarning("Nenhum ano encontrado para {Marca} {Modelo}.", marcaNome, modeloNome);
                    return null;
                }

                // 4. Busca o preço FIPE
                var precoUrl = $"{BaseUrl}/marcas/{marca.Codigo}/modelos/{modelo.Codigo}/anos/{anoEncontrado.Codigo}";
                var precoResponse = await _httpClient.GetAsync(precoUrl);
                precoResponse.EnsureSuccessStatusCode();

                var precoJson = await precoResponse.Content.ReadAsStringAsync();
                var preco = JsonSerializer.Deserialize<FipePrecoDto>(precoJson, JsonOptions);
                if (preco == null) return null;

                _logger.LogInformation("FIPE consultada com sucesso: {Marca} {Modelo} {Ano} = {Valor}",
                    preco.Marca, preco.Modelo, preco.AnoModelo, preco.Valor);

                // 5. Converte valor FIPE de "R$ 45.000,00" para decimal
                var valorNumerico = ConverterValorFipe(preco.Valor);

                // 6. Monta resposta base com dados da FIPE
                var resposta = new ConsultaFipeResponseDto
                {
                    CodigoFipe = preco.CodigoFipe,
                    Marca = preco.Marca,
                    Modelo = preco.Modelo,
                    AnoModelo = preco.AnoModelo,
                    Combustivel = preco.Combustivel,
                    ValorFipe = preco.Valor,
                    ValorFipeNumerico = valorNumerico,
                    MesReferencia = preco.MesReferencia
                };

                // 7. Enriquece com dados do nosso banco, se disponíveis
                var vehicleDb = await _context.Vehicles
                    .Where(v =>
                        EF.Functions.Like(v.Marca, $"%{marcaNome}%") &&
                        EF.Functions.Like(v.Modelo, $"%{modeloNome}%") &&
                        (v.AnoModelo == ano || v.AnoFabricacao == ano))
                    .FirstOrDefaultAsync();

                if (vehicleDb != null)
                {
                    var notaGeral = (vehicleDb.NotaConsumo + vehicleDb.NotaConforto + vehicleDb.NotaDesempenho +
                                    vehicleDb.NotaConfiabilidade + vehicleDb.NotaCustoManutencao + vehicleDb.NotaEspacoInterno) / 6;

                    resposta.DadosComplementares = new DadosComplementaresDto
                    {
                        VehicleId = vehicleDb.Id,
                        Versao = vehicleDb.Versao,
                        Motorizacao = vehicleDb.Motorizacao,
                        Aspiracao = vehicleDb.Aspiracao,
                        Cambio = vehicleDb.Cambio,
                        Categoria = vehicleDb.Categoria,
                        PaisOrigem = vehicleDb.PaisOrigem,
                        Importado = vehicleDb.Importado,
                        Descricao = vehicleDb.Descricao,
                        PontosPositivos = vehicleDb.PontosPositivos,
                        PontosNegativos = vehicleDb.PontosNegativos,
                        ProblemasCronicos = vehicleDb.ProblemasCronicos,
                        NotaConsumo = vehicleDb.NotaConsumo,
                        NotaConforto = vehicleDb.NotaConforto,
                        NotaDesempenho = vehicleDb.NotaDesempenho,
                        NotaConfiabilidade = vehicleDb.NotaConfiabilidade,
                        NotaCustoManutencao = vehicleDb.NotaCustoManutencao,
                        NotaEspacoInterno = vehicleDb.NotaEspacoInterno,
                        NotaGeral = Math.Round(notaGeral, 2)
                    };
                }

                return resposta;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erro ao consultar a API FIPE.");
                throw new Exception("Não foi possível consultar a API FIPE. Tente novamente mais tarde.", ex);
            }
        }

        // Encontra o item da lista cuja propriedade name mais se aproxima do texto buscado
        private static T? EncontrarMelhorCorrespondencia<T>(
            IEnumerable<T> lista,
            string busca,
            Func<T, string> getNome)
        {
            var buscaNorm = Normalizar(busca);

            // Tenta correspondência exata primeiro
            var exato = lista.FirstOrDefault(i => Normalizar(getNome(i)) == buscaNorm);
            if (exato != null) return exato;

            // Tenta "começa com"
            var comecaCom = lista.FirstOrDefault(i => Normalizar(getNome(i)).StartsWith(buscaNorm));
            if (comecaCom != null) return comecaCom;

            // Tenta "contém"
            return lista.FirstOrDefault(i => Normalizar(getNome(i)).Contains(buscaNorm));
        }

        private static string Normalizar(string texto)
        {
            return texto.ToLowerInvariant()
                        .Replace("-", " ")
                        .Replace("_", " ")
                        .Trim();
        }

        private static decimal ConverterValorFipe(string valor)
        {
            // Formato: "R$ 45.000,00"
            var limpo = valor.Replace("R$", "").Trim();
            if (decimal.TryParse(limpo, NumberStyles.Currency, new CultureInfo("pt-BR"), out var resultado))
                return resultado;
            return 0;
        }
    }
}
