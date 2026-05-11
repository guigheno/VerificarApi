namespace VerificarApi.Domain.Entities
{
    public class VehicleHistory
    {
        public Guid Id { get; set; }

        // Chave para identificar o veículo (marca+modelo+ano, não depende de cadastro)
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public int AnoModelo { get; set; }

        // Placa — opcional, preenchido quando consulta vem de placa
        public string? Placa { get; set; }

        public string TipoEvento { get; set; } = string.Empty; // Revisão, Acidente, Transferência, Multa, Recall
        public string Descricao { get; set; } = string.Empty;
        public DateTime DataEvento { get; set; }
        public string Fonte { get; set; } = string.Empty; // Manual, PlacaAPI, Sistema
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    }
}
