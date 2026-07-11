namespace VerificarApi.Domain.Entities
{
    // Veículo salvo pelo usuário a partir de uma consulta, para acesso rápido no
    // perfil e, futuramente, para o sistema de comparação entre veículos salvos.
    public class SavedVehicle
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public int AnoModelo { get; set; }
        public string? Placa { get; set; }

        // Snapshot da análise no momento em que foi salvo — evita ter que
        // reconsultar FIPE/IA só para listar os veículos salvos do usuário.
        public decimal ValorFipe { get; set; }
        public decimal? ValorEstimado { get; set; }
        public decimal? PercentualAjuste { get; set; }
        public decimal? NotaGeral { get; set; }
        public decimal? NotaConfiabilidade { get; set; }
        public decimal? NotaCustoManutencao { get; set; }
        public decimal? NotaConforto { get; set; }
        public decimal? NotaDesempenho { get; set; }
        public decimal? NotaConsumo { get; set; }
        public decimal? NotaEspacoInterno { get; set; }

        public DateTime SalvoEm { get; set; } = DateTime.UtcNow;
    }
}
