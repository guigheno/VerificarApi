namespace VerificarApi.Dtos.Fipe
{
    public class ConsultaFipeResponseDto
    {
        // Dados vindos da FIPE
        public string CodigoFipe { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public int AnoModelo { get; set; }
        public string Combustivel { get; set; } = string.Empty;
        public string ValorFipe { get; set; } = string.Empty;
        public decimal ValorFipeNumerico { get; set; }
        public string MesReferencia { get; set; } = string.Empty;

        // Dados complementares do nosso banco (se o veículo estiver cadastrado)
        public DadosComplementaresDto? DadosComplementares { get; set; }
    }

    public class DadosComplementaresDto
    {
        public Guid VehicleId { get; set; }
        public string Versao { get; set; } = string.Empty;
        public string Motorizacao { get; set; } = string.Empty;
        public string Aspiracao { get; set; } = string.Empty;
        public string Cambio { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string PaisOrigem { get; set; } = string.Empty;
        public bool Importado { get; set; }
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
        public decimal NotaGeral { get; set; }
    }

    // DTO para query params da consulta
    public class ConsultaFipeQueryDto
    {
        public string Marca { get; set; } = string.Empty;   // ex: "Chevrolet"
        public string Modelo { get; set; } = string.Empty;  // ex: "Onix"
        public int Ano { get; set; }                         // ex: 2022
    }
}
