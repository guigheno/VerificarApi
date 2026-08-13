using System.ComponentModel.DataAnnotations;

namespace VerificarApi.Dtos.RegistrationLinks
{
    public class RegistrationLinkResponseDto
    {
        public Guid Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public string? Nome { get; set; }
        public int QtdConsultaModelo { get; set; }
        public int QtdConsultaPlaca { get; set; }
        public bool Ativo { get; set; }
        public int LimiteUsos { get; set; }
        public int UsosAtuais { get; set; }
        public DateTime? ExpiraEm { get; set; }
        public DateTime CriadoEm { get; set; }
    }

    public class CreateRegistrationLinkDto
    {
        [MaxLength(100)]
        public string? Nome { get; set; }

        [Range(0, 1000000)]
        public int QtdConsultaModelo { get; set; }

        [Range(0, 1000000)]
        public int QtdConsultaPlaca { get; set; }

        [Range(0, 1000000)]
        public int LimiteUsos { get; set; }

        public DateTime? ExpiraEm { get; set; }
    }

    public class UpdateRegistrationLinkDto
    {
        [MaxLength(100)]
        public string? Nome { get; set; }

        [Range(0, 1000000)]
        public int QtdConsultaModelo { get; set; }

        [Range(0, 1000000)]
        public int QtdConsultaPlaca { get; set; }

        public bool Ativo { get; set; }

        [Range(0, 1000000)]
        public int LimiteUsos { get; set; }

        public DateTime? ExpiraEm { get; set; }
    }

    public class ValidarRegistrationLinkDto
    {
        public string Token { get; set; } = string.Empty;
        public string? Nome { get; set; }
        public int QtdConsultaModelo { get; set; }
        public int QtdConsultaPlaca { get; set; }
        public int LimiteUsos { get; set; }
        public int UsosAtuais { get; set; }
        public bool Valido { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
