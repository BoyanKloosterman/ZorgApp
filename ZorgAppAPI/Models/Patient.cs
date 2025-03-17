using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZorgAppAPI.Models
{

    public class Patient
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        [StringLength(50)]
        public string Voornaam { get; set; }

        [Required]
        [StringLength(50)]
        public string Achternaam { get; set; }

        [Required]
        public int OuderVoogd_ID { get; set; }

        [Required]
        public int TrajectID { get; set; }

        public int? ArtsID { get; set; }

        // Navigation properties
        [ForeignKey("OuderVoogd_ID")]
        public virtual OuderVoogd OuderVoogd { get; set; }

        [ForeignKey("TrajectID")]
        public virtual Traject Traject { get; set; }

        [ForeignKey("ArtsID")]
        public virtual Arts Arts { get; set; }
    }
}
