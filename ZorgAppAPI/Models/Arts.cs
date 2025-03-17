using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZorgAppAPI.Models
{
    public class Arts
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [StringLength(50)]
        public string Naam { get; set; }

        [Required]
        [StringLength(50)]
        public string Specialisatie { get; set; }

        // Navigation properties
        public virtual ICollection<Patient> Patienten { get; set; }
    }
}
