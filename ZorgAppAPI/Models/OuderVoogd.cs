using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZorgAppAPI.Models
{
    public class OuderVoogd
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        [StringLength(100)]
        public string Voornaam { get; set; }

        [Required]
        [StringLength(100)]
        public string Achternaam { get; set; }

        public string UserId { get; set; }

        // Navigation properties
        public virtual ICollection<Patient> Patienten { get; set; }
    }
}
