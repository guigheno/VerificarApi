using System.ComponentModel.DataAnnotations;

namespace VerificarApi.Dtos.Auth
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        [MaxLength(254)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(128)]
        public string Password { get; set; } = string.Empty;
    }
}