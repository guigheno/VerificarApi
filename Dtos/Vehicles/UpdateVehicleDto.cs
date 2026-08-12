namespace VerificarApi.Dtos.Vehicles
{
    public class UpdateVehicleDto
    {
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string Versao { get; set; } = string.Empty;
        public string Motorizacao { get; set; } = string.Empty;
        public string Aspiracao { get; set; } = string.Empty;
        public string Combustivel { get; set; } = string.Empty;
        public string Cambio { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string PaisOrigem { get; set; } = string.Empty;
        public bool Importado { get; set; }
        public int AnoFabricacao { get; set; }
        public int AnoModelo { get; set; }
        public double FipeValor { get; set; }
        public DateTime FipeData { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public string PontosPositivos { get; set; } = string.Empty;
        public string PontosNegativos { get; set; } = string.Empty;
        public string ProblemasCronicos { get; set; } = string.Empty;
        public decimal NotaConsumo { get; set; }
        public decimal NotaConforto { get; set; }
        public decimal NotaDesempenho { get; set; }
        public decimal NotaConfiabilidade { get; set; }
        public decimal NotaCustoManutencao { get; set; }
        public decimal NotaEspacoInterno { get; set; }
    }
}