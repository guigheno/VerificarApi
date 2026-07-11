namespace VerificarApi.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        public Guid TenantId { get; set; } // ver nota em TenantProvider: não há isolamento real por tenant hoje
    }
}