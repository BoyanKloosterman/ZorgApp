using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ZorgAppAPI.Models
{
    public class Notificatie
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Bericht { get; set; } = string.Empty;

        public bool IsGelezen { get; set; } = false;

        [Required]
        public DateTime DatumAanmaak { get; set; }

        [Required]
        public DateTime DatumVerloop { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }
    }
}
