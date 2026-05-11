using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Text.Json;
using VerificarApi.Domain.Entities;
using VerificarApi.Dtos.Fipe;
using VerificarApi.Infrastructure.Data;
using VerificarApi.Services.Interfaces;

namespace VerificarApi.Services
{
    public class FipeCacheService : IFipeCacheService
    {
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly ILogger<FipeCacheService> _logger;

        private const string BaseUrl = "https://parallelum.com.br/fipe/api/v1/carros";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public FipeCacheService(AppDbContext context, HttpClient httpClient, ILogger<FipeCacheService> logger)
        {
            _context = context;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<bool> CachePopuladoAsync()
        {
            return await _context.FipeMarcas.AnyAsync();
        }

        public async Task<List<FipeMarcaDto>> ListarMarcasAsync()
        {
            return await _context.FipeMarcas
                .OrderBy(m => m.Nome)
                .Select(m => new FipeMarcaDto
                {
                    Codigo = m.Codigo,
                    Nome = m.Nome
                })
                .ToListAsync();
        }

        public async Task<List<FipeModeloDto>> ListarModelosPorMarcaAsync(string codigoMarca)
        {
            return await _context.FipeModelos
                .Where(m => m.CodigoMarca == codigoMarca)
                .OrderBy(m => m.Nome)
                .Select(m => new FipeModeloDto
                {
                    Codigo = m.Codigo,
                    Nome = m.Nome
                })
                .ToListAsync();
        }

        public async Task<FipeSyncResultDto> SincronizarAsync()
        {
            var sw = Stopwatch.StartNew();
            var resultado = new FipeSyncResultDto();

            _logger.LogInformation("Iniciando sincronização FIPE...");

            // 1. Busca todas as marcas da API
            List<FipeMarcaDto> marcasApi;
            try
            {
                var json = await _httpClient.GetStringAsync($"{BaseUrl}/marcas");
                marcasApi = JsonSerializer.Deserialize<List<FipeMarcaDto>>(json, JsonOptions) ?? new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao buscar marcas da API FIPE.");
                resultado.Erros.Add($"Falha ao buscar marcas: {ex.Message}");
                return resultado;
            }

            // 2. Limpa o cache antigo
            _logger.LogInformation("Limpando cache antigo...");
            await _context.FipeModelos.ExecuteDeleteAsync();
            await _context.FipeMarcas.ExecuteDeleteAsync();

            // 3. Insere as marcas novas
            var marcasEntidade = marcasApi.Select(m => new FipeMarca
            {
                Codigo = m.Codigo,
                Nome = m.Nome
            }).ToList();

            await _context.FipeMarcas.AddRangeAsync(marcasEntidade);
            await _context.SaveChangesAsync();
            resultado.MarcasSincronizadas = marcasEntidade.Count;
            _logger.LogInformation("{Total} marcas salvas. Buscando modelos...", marcasEntidade.Count);

            // 4. Para cada marca, busca e salva os modelos em lote
            var todosModelos = new List<FipeModelo>();
            int marcasProcessadas = 0;

            foreach (var marca in marcasApi)
            {
                try
                {
                    var url = $"{BaseUrl}/marcas/{marca.Codigo}/modelos";
                    var json = await _httpClient.GetStringAsync(url);
                    var modelosData = JsonSerializer.Deserialize<FipeModelosResponseDto>(json, JsonOptions);

                    if (modelosData?.Modelos != null)
                    {
                        foreach (var m in modelosData.Modelos)
                        {
                            todosModelos.Add(new FipeModelo
                            {
                                Codigo = m.Codigo,
                                Nome = m.Nome,
                                CodigoMarca = marca.Codigo,
                                NomeMarca = marca.Nome
                            });
                        }
                    }

                    marcasProcessadas++;

                    // Salva em lotes de 20 marcas para não acumular tudo na memória
                    if (marcasProcessadas % 20 == 0)
                    {
                        await _context.FipeModelos.AddRangeAsync(todosModelos);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("Progresso: {Processadas}/{Total} marcas | {Modelos} modelos salvos",
                            marcasProcessadas, marcasApi.Count, todosModelos.Count);
                        resultado.ModelosSincronizados += todosModelos.Count;
                        todosModelos.Clear();
                    }

                    // Pequena pausa para não sobrecarregar a API pública
                    await Task.Delay(100);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Erro ao buscar modelos da marca {Marca}: {Erro}", marca.Nome, ex.Message);
                    resultado.Erros.Add($"Marca '{marca.Nome}': {ex.Message}");
                }
            }

            // Salva o restante que ficou no lote
            if (todosModelos.Any())
            {
                await _context.FipeModelos.AddRangeAsync(todosModelos);
                await _context.SaveChangesAsync();
                resultado.ModelosSincronizados += todosModelos.Count;
            }

            sw.Stop();
            resultado.TempoTotal = sw.Elapsed;

            _logger.LogInformation(
                "Sincronização concluída em {Tempo}. Marcas: {Marcas} | Modelos: {Modelos} | Erros: {Erros}",
                sw.Elapsed, resultado.MarcasSincronizadas, resultado.ModelosSincronizados, resultado.Erros.Count);

            return resultado;
        }
    }
}
