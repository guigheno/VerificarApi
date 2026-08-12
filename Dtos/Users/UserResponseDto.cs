namespace VerificarApi.Dtos.Users
{
    public class UserResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;
        public int QtdConsultaModelo { get; set; }
        public int QtdConsultaPlaca { get; set; }
    }
}