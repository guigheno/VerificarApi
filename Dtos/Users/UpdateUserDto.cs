using System.ComponentModel.DataAnnotations;

namespace VerificarApi.Dtos.Users
{
    public class UpdateUserDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [Required]
        [EmailAddress]
        [MaxLength(254)]
        public string Email { get; set; } = null!;

        [Required]
        [RegularExpression("^(Admin|User)$")]
        public string Role { get; set; } = null!;
    }
}