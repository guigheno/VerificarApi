namespace VerificarApi.Domain.Entities
{
    public class VehicleAnalysis
    {
        public Guid Id { get; set; }

        // Chave de identificação única do modelo analisado
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public int AnoModelo { get; set; }

        // Resultado da análise da IA
        public string ProblemasCronicos { get; set; } = string.Empty;
        public string PontosPositivos { get; set; } = string.Empty;
        public string PontosNegativos { get; set; } = string.Empty;
        public string ResumoGeral { get; set; } = string.Empty;

        // Notas de 0 a 5
        public decimal NotaConfiabilidade { get; set; }
        public decimal NotaCustoManutencao { get; set; }
        public decimal NotaConforto { get; set; }
        public decimal NotaDesempenho { get; set; }
        public decimal NotaConsumo { get; set; }
        public decimal NotaEspacoInterno { get; set; }

        // Controle
        public DateTime GeradoEm { get; set; } = DateTime.UtcNow;
        public DateTime UltimaConsultaEm { get; set; } = DateTime.UtcNow;
        public DateTime ProximaAtualizacaoEm { get; set; }
        public string ModeloIA { get; set; } = string.Empty; // ex: gpt-4o-mini
    }
}
