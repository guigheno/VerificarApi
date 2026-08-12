using System.ComponentModel.DataAnnotations;

namespace VerificarApi.Dtos.Users
{
    public class CreateUserDto
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

        [RegularExpression("^(Admin|User)$")]
        public string Role { get; set; } = "User";
    }
}