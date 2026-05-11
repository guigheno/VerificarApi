using VerificarApi.Dtos.Analise;
using VerificarApi.Services.Interfaces;

namespace VerificarApi.Services
{
    public class PrecificacaoService : IPrecificacaoService
    {
        private readonly ILogger<PrecificacaoService> _logger;

        public PrecificacaoService(ILogger<PrecificacaoService> logger)
        {
            _logger = logger;
        }

        public PrecificacaoResponseDto Calcular(PrecificacaoRequestDto req)
        {
            _logger.LogInformation("Calculando preço para {Marca} {Modelo} {Ano} | {Km}km | {Cond}",
                req.Marca, req.Modelo, req.AnoModelo, req.Quilometragem, req.Condicao);

            var fatores = new List<string>();
            decimal multiplicador = 1.0m;
            int anoAtual = DateTime.Now.Year;
            int idadeVeiculo = anoAtual - req.AnoModelo;

            // 2. Fator de quilometragem
            // Referência: 15.000 km/ano é considerado normal
            int kmEsperado = idadeVeiculo * 15000;
            int kmDiferenca = req.Quilometragem - kmEsperado;

            decimal fatorKm = 0;
            if (kmDiferenca > 50000)
            {
                fatorKm = -0.10m;
                fatores.Add($"Quilometragem muito acima do esperado ({req.Quilometragem:N0} km): -{Math.Abs(fatorKm):P0}");
            }
            else if (kmDiferenca > 20000)
            {
                fatorKm = -0.05m;
                fatores.Add($"Quilometragem acima do esperado ({req.Quilometragem:N0} km): -{Math.Abs(fatorKm):P0}");
            }
            else if (kmDiferenca < -20000)
            {
                fatorKm = 0.05m;
                fatores.Add($"Quilometragem abaixo do esperado ({req.Quilometragem:N0} km): +{fatorKm:P0}");
            }
            else
            {
                fatores.Add($"Quilometragem dentro do esperado ({req.Quilometragem:N0} km)");
            }
            multiplicador += fatorKm;

            // 3. Fator de condição
            decimal fatorCondicao = req.Condicao?.ToLower() switch
            {
                "excelente" => 0.05m,
                "bom"       => 0.0m,
                "regular"   => -0.08m,
                "ruim"      => -0.18m,
                _           => 0.0m
            };
            if (fatorCondicao != 0)
                fatores.Add($"Condição {req.Condicao}: {(fatorCondicao > 0 ? "+" : "")}{fatorCondicao:P0}");
            else
                fatores.Add($"Condição {req.Condicao}: sem ajuste");
            multiplicador += fatorCondicao;

            // Garante mínimo de 20% do valor FIPE
            multiplicador = Math.Max(multiplicador, 0.20m);

            decimal valorEstimado = Math.Round(req.ValorFipe * multiplicador, 2);
            decimal percentualAjuste = Math.Round((multiplicador - 1) * 100, 1);

            // Faixa de negociação (±7%)
            decimal faixaMin = Math.Round(valorEstimado * 0.93m, 2);
            decimal faixaMax = Math.Round(valorEstimado * 1.07m, 2);

            string recomendacao = percentualAjuste switch
            {
                >= 0  => "Veículo com boa valorização em relação à tabela FIPE.",
                >= -10 => "Valor alinhado com a tabela FIPE. Negocie dentro da faixa sugerida.",
                >= -25 => "Veículo com desconto relevante. Verifique o estado de conservação.",
                _      => "Desconto significativo. Avalie cuidadosamente antes de comprar."
            };

            return new PrecificacaoResponseDto
            {
                ValorFipe = req.ValorFipe,
                ValorEstimado = valorEstimado,
                PercentualAjuste = percentualAjuste,
                FaixaMinima = faixaMin.ToString("C", new System.Globalization.CultureInfo("pt-BR")),
                FaixaMaxima = faixaMax.ToString("C", new System.Globalization.CultureInfo("pt-BR")),
                Recomendacao = recomendacao,
                FatoresAjuste = fatores
            };
        }
    }
}
