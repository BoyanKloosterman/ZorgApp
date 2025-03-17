using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ZorgAppAPI.Models
{
    public class Traject_ZorgMoment
    {
        [Key, Column(Order = 0)]
        public int TrajectID { get; set; }

        [Key, Column(Order = 1)]
        public int ZorgMomentID { get; set; }

        [Required]
        public int Volgorde { get; set; }

        // Navigation properties
        [ForeignKey("TrajectID")]
        public virtual Traject Traject { get; set; }

        [ForeignKey("ZorgMomentID")]
        public virtual ZorgMoment ZorgMoment { get; set; }
    }
}
