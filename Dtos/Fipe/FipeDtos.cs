using System.Text.Json.Serialization;

namespace VerificarApi.Dtos.Fipe
{
    // Resposta de /marcas
    public class FipeMarcaDto
    {
        [JsonPropertyName("codigo")]
        public string Codigo { get; set; } = string.Empty;

        [JsonPropertyName("nome")]
        public string Nome { get; set; } = string.Empty;
    }

    // Resposta de /marcas/{codigo}/modelos
    public class FipeModelosResponseDto
    {
        [JsonPropertyName("modelos")]
        public List<FipeModeloDto> Modelos { get; set; } = new();

        [JsonPropertyName("anos")]
        public List<FipeAnoDto> Anos { get; set; } = new();
    }

    public class FipeModeloDto
    {
        [JsonPropertyName("codigo")]
        public int Codigo { get; set; }

        [JsonPropertyName("nome")]
        public string Nome { get; set; } = string.Empty;
    }

    // Resposta de /marcas/{codigo}/modelos/{modeloCodigo}/anos
    public class FipeAnoDto
    {
        [JsonPropertyName("codigo")]
        public string Codigo { get; set; } = string.Empty;

        [JsonPropertyName("nome")]
        public string Nome { get; set; } = string.Empty;
    }

    // Resposta final com o preço
    public class FipePrecoDto
    {
        [JsonPropertyName("TipoVeiculo")]
        public int TipoVeiculo { get; set; }

        [JsonPropertyName("Valor")]
        public string Valor { get; set; } = string.Empty;

        [JsonPropertyName("Marca")]
        public string Marca { get; set; } = string.Empty;

        [JsonPropertyName("Modelo")]
        public string Modelo { get; set; } = string.Empty;

        [JsonPropertyName("AnoModelo")]
        public int AnoModelo { get; set; }

        [JsonPropertyName("Combustivel")]
        public string Combustivel { get; set; } = string.Empty;

        [JsonPropertyName("CodigoFipe")]
        public string CodigoFipe { get; set; } = string.Empty;

        [JsonPropertyName("MesReferencia")]
        public string MesReferencia { get; set; } = string.Empty;

        [JsonPropertyName("SiglaCombustivel")]
        public string SiglaCombustivel { get; set; } = string.Empty;
    }
}
