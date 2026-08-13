namespace VerificarApi.Domain.Entities
{
    public class RegistrationLink
    {
        public Guid Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public string? Nome { get; set; }
        public int QtdConsultaModelo { get; set; }
        public int QtdConsultaPlaca { get; set; }
        public bool Ativo { get; set; } = true;
        public int LimiteUsos { get; set; }
        public int UsosAtuais { get; set; }
        public DateTime? ExpiraEm { get; set; }
        public DateTime CriadoEm { get; set; }
    }
}
