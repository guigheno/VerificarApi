namespace VerificarApi.Dtos.Analise
{
    // --- Histórico (2.3) ---
    public class VehicleHistoryDto
    {
        public Guid Id { get; set; }
        public string TipoEvento { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public DateTime DataEvento { get; set; }
        public string Fonte { get; set; } = string.Empty;
    }

    public class CreateHistoryDto
    {
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public int AnoModelo { get; set; }
        public string? Placa { get; set; }
        public string TipoEvento { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public DateTime DataEvento { get; set; }
    }

    // --- Precificação (2.4) ---
    public class PrecificacaoRequestDto
    {
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public int AnoModelo { get; set; }
        public decimal ValorFipe { get; set; }
        public int Quilometragem { get; set; }
        public string Condicao { get; set; } = "Não sei"; // Excelente, Bom, Regular, Ruim, Não sei
        // Campos adicionais (Ciclo 3)
        public string Combustivel { get; set; } = "Flex"; // Gasolina, Etanol, Flex, Diesel, Elétrico, Híbrido
        public string Cambio { get; set; } = "Não sei"; // Manual, Automático, CVT, Não sei
        public string NumeroProprietarios { get; set; } = "Não sei"; // "1", "2", "3+", "Não sei"
        public string IpvaEmDia { get; set; } = "Não sei"; // Sim, Não, Não sei
        public string RevisaoEmDia { get; set; } = "Não sei"; // Sim, Não, Não sei
        public string DocumentacaoRegular { get; set; } = "Não sei"; // Sim, Não, Não sei
        public string Sinistrado { get; set; } = "Não sei"; // Sim, Não, Não sei — recuperado de leilão ou colisão
        public string EstadoPintura { get; set; } = "Não sei"; // Original, Repintura parcial, Repintura total, Não sei
        public string EstadoInterior { get; set; } = "Não sei"; // Bom, Desgastado, Danificado, Não sei
        public string EstadoPneus { get; set; } = "Não sei"; // Novos, Meia vida, Precisam troca, Não sei
        public string EstadoRodas { get; set; } = "Não sei"; // Sem avarias, Amassadas/arranhadas, Não sei
    }

    public class PrecificacaoResponseDto
    {
        public decimal ValorFipe { get; set; }
        public decimal ValorEstimado { get; set; }
        public decimal PercentualAjuste { get; set; }
        public string FaixaMinima { get; set; } = string.Empty;
        public string FaixaMaxima { get; set; } = string.Empty;
        public string Recomendacao { get; set; } = string.Empty;
        public List<string> FatoresAjuste { get; set; } = new();
    }

    // --- Análise IA (2.5) ---
    public class VehicleAnalysisDto
    {
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public int AnoModelo { get; set; }
        public string ProblemasCronicos { get; set; } = string.Empty;
        public string PontosPositivos { get; set; } = string.Empty;
        public string PontosNegativos { get; set; } = string.Empty;
        public string ResumoGeral { get; set; } = string.Empty;
        public decimal NotaConfiabilidade { get; set; }
        public decimal NotaCustoManutencao { get; set; }
        public decimal NotaConforto { get; set; }
        public decimal NotaDesempenho { get; set; }
        public decimal NotaConsumo { get; set; }
        public decimal NotaEspacoInterno { get; set; }
        public decimal NotaGeral { get; set; }
        public bool GeradoPorIA { get; set; }
        public DateTime GeradoEm { get; set; }
        public DateTime UltimaConsultaEm { get; set; }
        public DateTime ProximaAtualizacaoEm { get; set; }
    }

    // --- Resposta consolidada (2.6) ---
    public class AnaliseCompletaResponseDto
    {
        // Identificação
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public int AnoModelo { get; set; }

        // FIPE
        public string ValorFipe { get; set; } = string.Empty;
        public decimal ValorFipeNumerico { get; set; }
        public string MesReferencia { get; set; } = string.Empty;

        // Precificação
        public PrecificacaoResponseDto? Precificacao { get; set; }

        // Histórico
        public List<VehicleHistoryDto> Historico { get; set; } = new();

        // Análise de IA
        public VehicleAnalysisDto? Analise { get; set; }

        // Dados complementares do banco (se cadastrado)
        public bool PossuiDadosCadastrados { get; set; }
    }
}
