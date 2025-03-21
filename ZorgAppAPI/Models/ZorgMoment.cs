using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ZorgAppAPI.Models
{
    public class ZorgMoment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        [StringLength(50)]
        public string Naam { get; set; }

        public string Tekst { get; set; }

        [StringLength(256)]
        public string Url { get; set; }

        public byte[] Plaatje { get; set; }

        public int? TijdsDuurInMin { get; set; }

        // Navigation properties
        public virtual ICollection<Traject_ZorgMoment> TrajectZorgMomenten { get; set; }
    }
}
