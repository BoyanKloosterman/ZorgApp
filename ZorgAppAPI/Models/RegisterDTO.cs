using System.ComponentModel.DataAnnotations;

namespace ZorgAppAPI.Models
{
    public class RegisterDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{6,}$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number and one special character")]
        public string Password { get; set; }

        [Required]
        [StringLength(100)]
        public string Voornaam { get; set; }

        [Required]
        [StringLength(100)]
        public string Achternaam { get; set; }

        [Required]
        [RegularExpression("^(Arts|Patient|OuderVoogd)$")]
        public string Role { get; set; }
    }
}
