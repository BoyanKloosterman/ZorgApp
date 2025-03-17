using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZorgAppAPI.Models
{
    public class Traject
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        [StringLength(50)]
        public string Naam { get; set; }
        [StringLength(256)]
        public string Omschrijving { get; set; }

        // Navigation properties
        public virtual ICollection<Patient> Patienten { get; set; }
        public virtual ICollection<Traject_ZorgMoment> TrajectZorgMomenten { get; set; }
    }
}
