using System.Text.Json.Serialization;

namespace VerificarApi.Dtos.Placa
{
    // DTO principal de resposta para o cliente
    public class ConsultaPlacaResponseDto
    {
        public string Placa { get; set; } = string.Empty;
        public string PlacaAlternativa { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string Versao { get; set; } = string.Empty;
        public string AnoFabricacao { get; set; } = string.Empty;
        public string AnoModelo { get; set; } = string.Empty;
        public string Cor { get; set; } = string.Empty;
        public string Combustivel { get; set; } = string.Empty;
        public string Municipio { get; set; } = string.Empty;
        public string Uf { get; set; } = string.Empty;
        public string Origem { get; set; } = string.Empty;
        public string Situacao { get; set; } = string.Empty;
        public string TipoVeiculo { get; set; } = string.Empty;
        public string Segmento { get; set; } = string.Empty;
        public string Cilindradas { get; set; } = string.Empty;
        public string QuantidadePassageiros { get; set; } = string.Empty;

        // Restrições
        public List<string> Restricoes { get; set; } = new();

        // Melhor correspondência FIPE (maior score)
        public FipeMatchDto? Fipe { get; set; }

        // Logo da marca
        public string? LogoUrl { get; set; }

        // Data/hora da consulta
        public string DataConsulta { get; set; } = string.Empty;
    }

    public class FipeMatchDto
    {
        public string CodigoFipe { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string Valor { get; set; } = string.Empty;
        public string Combustivel { get; set; } = string.Empty;
        public string AnoModelo { get; set; } = string.Empty;
        public string MesReferencia { get; set; } = string.Empty;
        public int Score { get; set; }
    }

    // DTOs internos para deserializar a resposta da API externa
    public class ApiPlacaResponseDto
    {
        [JsonPropertyName("MARCA")] public string Marca { get; set; } = string.Empty;
        [JsonPropertyName("MODELO")] public string Modelo { get; set; } = string.Empty;
        [JsonPropertyName("SUBMODELO")] public string Submodelo { get; set; } = string.Empty;
        [JsonPropertyName("VERSAO")] public string Versao { get; set; } = string.Empty;
        [JsonPropertyName("ano")] public string Ano { get; set; } = string.Empty;
        [JsonPropertyName("anoModelo")] public string AnoModelo { get; set; } = string.Empty;
        [JsonPropertyName("cor")] public string Cor { get; set; } = string.Empty;
        [JsonPropertyName("situacao")] public string Situacao { get; set; } = string.Empty;
        [JsonPropertyName("municipio")] public string Municipio { get; set; } = string.Empty;
        [JsonPropertyName("origem")] public string Origem { get; set; } = string.Empty;
        [JsonPropertyName("placa")] public string Placa { get; set; } = string.Empty;
        [JsonPropertyName("placa_alternativa")] public string PlacaAlternativa { get; set; } = string.Empty;
        [JsonPropertyName("segmento")] public string Segmento { get; set; } = string.Empty;
        [JsonPropertyName("sub_segmento")] public string SubSegmento { get; set; } = string.Empty;
        [JsonPropertyName("mensagemRetorno")] public string MensagemRetorno { get; set; } = string.Empty;
        [JsonPropertyName("logo")] public string Logo { get; set; } = string.Empty;
        [JsonPropertyName("data")] public string Data { get; set; } = string.Empty;
        [JsonPropertyName("extra")] public ApiPlacaExtraDto? Extra { get; set; }
        [JsonPropertyName("fipe")] public ApiPlacaFipeDto? Fipe { get; set; }
    }

    public class ApiPlacaExtraDto
    {
        [JsonPropertyName("restricao_1")] public string Restricao1 { get; set; } = string.Empty;
        [JsonPropertyName("restricao_2")] public string Restricao2 { get; set; } = string.Empty;
        [JsonPropertyName("restricao_3")] public string Restricao3 { get; set; } = string.Empty;
        [JsonPropertyName("restricao_4")] public string Restricao4 { get; set; } = string.Empty;
        [JsonPropertyName("combustivel")] public string Combustivel { get; set; } = string.Empty;
        [JsonPropertyName("cilindradas")] public string Cilindradas { get; set; } = string.Empty;
        [JsonPropertyName("quantidade_passageiro")] public string QuantidadePassageiro { get; set; } = string.Empty;
        [JsonPropertyName("tipo_veiculo")] public string TipoVeiculo { get; set; } = string.Empty;
        [JsonPropertyName("ano_fabricacao")] public string AnoFabricacao { get; set; } = string.Empty;
        [JsonPropertyName("ano_modelo")] public string AnoModelo { get; set; } = string.Empty;
        [JsonPropertyName("municipio")] public string Municipio { get; set; } = string.Empty;
        [JsonPropertyName("uf")] public string Uf { get; set; } = string.Empty;
        [JsonPropertyName("situacao_veiculo")] public string SituacaoVeiculo { get; set; } = string.Empty;
    }

    public class ApiPlacaFipeDto
    {
        [JsonPropertyName("dados")] public List<ApiPlacaFipeDadoDto> Dados { get; set; } = new();
    }

    public class ApiPlacaFipeDadoDto
    {
        [JsonPropertyName("codigo_fipe")] public string CodigoFipe { get; set; } = string.Empty;
        [JsonPropertyName("texto_modelo")] public string TextoModelo { get; set; } = string.Empty;
        [JsonPropertyName("texto_valor")] public string TextoValor { get; set; } = string.Empty;
        [JsonPropertyName("combustivel")] public string Combustivel { get; set; } = string.Empty;
        [JsonPropertyName("ano_modelo")] public string AnoModelo { get; set; } = string.Empty;
        [JsonPropertyName("mes_referencia")] public string MesReferencia { get; set; } = string.Empty;
        [JsonPropertyName("score")] public int Score { get; set; }
    }
}
