namespace VerificarApi.Dtos.SavedVehicles
{
    public class CreateSavedVehicleDto
    {
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public int AnoModelo { get; set; }
        public string? Placa { get; set; }
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
    }

    public class SavedVehicleResponseDto
    {
        public Guid Id { get; set; }
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public int AnoModelo { get; set; }
        public string? Placa { get; set; }
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
        public DateTime SalvoEm { get; set; }
    }
}
