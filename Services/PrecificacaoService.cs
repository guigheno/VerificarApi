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

        // Campos tri-state (Sim/Não/Não sei): "Não sei" nunca penaliza nem beneficia —
        // só some/falta de informação não deve virar suposição no cálculo.
        private static bool EhSim(string? valor) => string.Equals(valor, "Sim", StringComparison.OrdinalIgnoreCase);
        private static bool EhNao(string? valor) =>
            string.Equals(valor, "Não", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(valor, "Nao", StringComparison.OrdinalIgnoreCase);

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

            // 4. Fator de câmbio
            decimal fatorCambio = req.Cambio?.ToLower() switch
            {
                "automático" or "automatico" or "cvt" => 0.04m,
                "semi-automático" or "semi-automatico" => 0.02m,
                _ => 0.0m
            };
            if (fatorCambio != 0)
                fatores.Add($"Câmbio {req.Cambio}: +{fatorCambio:P0}");
            multiplicador += fatorCambio;

            // 5. Fator de número de proprietários ("Não sei" = neutro, sem alegar dono único)
            decimal fatorProprietarios = req.NumeroProprietarios switch
            {
                "1" => 0.0m,
                "2" => -0.03m,
                "3+" => -0.07m,
                _ => 0.0m
            };
            if (req.NumeroProprietarios == "1")
                fatores.Add("1 único proprietário: sem ajuste");
            else if (fatorProprietarios != 0)
                fatores.Add($"{req.NumeroProprietarios} proprietários anteriores: {fatorProprietarios:P0}");
            multiplicador += fatorProprietarios;

            // 6. Fator de IPVA (só penaliza quando o usuário sabe e informa que NÃO está em dia)
            if (EhNao(req.IpvaEmDia))
            {
                multiplicador -= 0.02m;
                fatores.Add("IPVA não está em dia: -2%");
            }

            // 7. Fator de revisão
            if (EhSim(req.RevisaoEmDia))
            {
                multiplicador += 0.03m;
                fatores.Add("Revisão em dia (histórico de manutenção): +3%");
            }
            else if (EhNao(req.RevisaoEmDia))
            {
                multiplicador -= 0.03m;
                fatores.Add("Revisão não está em dia: -3%");
            }

            // 8. Fator de documentação
            if (EhNao(req.DocumentacaoRegular))
            {
                multiplicador -= 0.10m;
                fatores.Add("Documentação irregular: -10%");
            }

            // 9. Fator de sinistro/leilão — maior impacto isolado no valor de mercado,
            // pois reduz tanto o valor de revenda quanto a confiança do comprador
            if (EhSim(req.Sinistrado))
            {
                multiplicador -= 0.18m;
                fatores.Add("Histórico de sinistro/leilão: -18%");
            }

            // 10. Fator de estado da pintura — repintura costuma indicar reparo de
            // colisão e reduz o valor percebido pelo comprador
            decimal fatorPintura = req.EstadoPintura?.ToLower() switch
            {
                "repintura parcial" => -0.05m,
                "repintura total" => -0.12m,
                _ => 0.0m
            };
            if (fatorPintura != 0)
                fatores.Add($"Pintura ({req.EstadoPintura}): {fatorPintura:P0}");
            multiplicador += fatorPintura;

            // 11. Fator de estado do interior
            decimal fatorInterior = req.EstadoInterior?.ToLower() switch
            {
                "desgastado" => -0.04m,
                "danificado" => -0.08m,
                _ => 0.0m
            };
            if (fatorInterior != 0)
                fatores.Add($"Interior ({req.EstadoInterior}): {fatorInterior:P0}");
            multiplicador += fatorInterior;

            // 12. Fator de estado dos pneus — custo imediato que o comprador terá que assumir
            decimal fatorPneus = req.EstadoPneus?.ToLower() switch
            {
                "novos" => 0.02m,
                "precisam troca" => -0.04m,
                _ => 0.0m
            };
            if (fatorPneus != 0)
                fatores.Add($"Pneus ({req.EstadoPneus}): {(fatorPneus > 0 ? "+" : "")}{fatorPneus:P0}");
            multiplicador += fatorPneus;

            // 13. Fator de estado das rodas
            decimal fatorRodas = req.EstadoRodas?.ToLower() switch
            {
                "amassadas/arranhadas" => -0.03m,
                _ => 0.0m
            };
            if (fatorRodas != 0)
                fatores.Add($"Rodas ({req.EstadoRodas}): {fatorRodas:P0}");
            multiplicador += fatorRodas;

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
