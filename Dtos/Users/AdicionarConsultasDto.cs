using System.ComponentModel.DataAnnotations;

namespace VerificarApi.Dtos.Users
{
    public class AdicionarConsultasDto
    {
        [Range(0, int.MaxValue)]
        public int QtdConsultaModelo { get; set; }

        [Range(0, int.MaxValue)]
        public int QtdConsultaPlaca { get; set; }
    }
}
