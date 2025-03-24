using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ZorgAppAPI.Models
{
    public class Notitie
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required(ErrorMessage = "De titel is verplicht.")]
        [StringLength(30, ErrorMessage = "De titel mag maximaal 30 tekens lang zijn.")]
        public string Titel { get; set; }

        [StringLength(255, ErrorMessage = "De tekst mag maximaal 255 tekens lang zijn.")]
        public string Tekst { get; set; }

        [ForeignKey("User")] // Link naar de AspNetUsers tabel
        public string UserId { get; set; }

        [Required]
        public DateTime DatumAanmaak { get; set; }
    }
}
