namespace VerificarApi.Domain.Entities
{
    public class FipeModelo
    {
        public int Id { get; set; }
        public int Codigo { get; set; }           // ex: 4859
        public string Nome { get; set; } = string.Empty;  // ex: "Punto T-JET 1.4 16V Turbo 5p"
        public string CodigoMarca { get; set; } = string.Empty; // ex: "21"
        public string NomeMarca { get; set; } = string.Empty;   // ex: "Fiat"
    }
}
