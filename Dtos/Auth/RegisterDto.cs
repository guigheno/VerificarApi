using System.ComponentModel.DataAnnotations;

namespace VerificarApi.Dtos.Auth
{
    public class RegisterDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(254)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        [MaxLength(128)]
        public string Password { get; set; } = string.Empty;

        [MaxLength(64)]
        public string? CodigoLink { get; set; }

        public string? RecaptchaToken { get; set; }
    }
}