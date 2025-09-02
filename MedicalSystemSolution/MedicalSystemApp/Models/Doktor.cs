using System.ComponentModel.DataAnnotations;

namespace MedicalSystemApp.Models
{
    public class Doktor
    {
        [Key]
        public int DoktorId { get; set; }

        [Required, MaxLength(100)]
        public string Ime { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Prezime { get; set; } = string.Empty;
    }
}
