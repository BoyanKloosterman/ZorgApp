using System;
using System.ComponentModel.DataAnnotations;

namespace ZorgAppAPI.Models
{
    public class PatientRegisterDTO
    {
        [Required(ErrorMessage = "Voornaam is verplicht")]
        public string Voornaam { get; set; }

        [Required(ErrorMessage = "Achternaam is verplicht")]
        public string Achternaam { get; set; }

        [Required(ErrorMessage = "Email is verplicht")]
        [EmailAddress(ErrorMessage = "Ongeldig email formaat")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Wachtwoord is verplicht")]
        [MinLength(6, ErrorMessage = "Wachtwoord moet minimaal 6 tekens bevatten")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Geboortedatum is verplicht")]
        public DateTime GeboorteDatum { get; set; }

        [Required(ErrorMessage = "Avatar ID is verplicht")]
        public int AvatarId { get; set; }

        // Optional fields
        public int? OuderVoogdId { get; set; }
        public int? TrajectId { get; set; }
        public int? ArtsId { get; set; }
    }
}