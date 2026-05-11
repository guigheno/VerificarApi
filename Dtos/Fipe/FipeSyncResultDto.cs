namespace VerificarApi.Dtos.Fipe
{
    public class FipeSyncResultDto
    {
        public int MarcasSincronizadas { get; set; }
        public int ModelosSincronizados { get; set; }
        public List<string> Erros { get; set; } = new();
        public TimeSpan TempoTotal { get; set; }
        public DateTime SincronizadoEm { get; set; } = DateTime.UtcNow;
    }
}
