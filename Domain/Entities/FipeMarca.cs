namespace VerificarApi.Domain.Entities
{
    public class FipeMarca
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;  // ex: "21"
        public string Nome { get; set; } = string.Empty;    // ex: "Fiat"
    }
}
