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

        public int OuderVoogdID { get; set; }

        public int? TrajectID { get; set; }

        public int? ArtsID { get; set; }

        public string Geboortedatum { get; set; }

        public string UserId { get; set; }
        public int AvatarID { set; get; }
        // Navigation properties
        //[ForeignKey("OuderVoogd_ID")]
        //public virtual OuderVoogd OuderVoogd { get; set; }

        //[ForeignKey("TrajectID")]
        //public virtual Traject Traject { get; set; }

        //[ForeignKey("ArtsID")]
        //public virtual Arts Arts { get; set; }
    }
}
