using System.ComponentModel.DataAnnotations;

namespace MedicalSystemApp.Models
{
    public class Lijek
    {
        [Key]
        public int LijekId { get; set; }

        [Required, MaxLength(100)]
        public string Naziv { get; set; } = string.Empty;

        public string? Opis { get; set; }
    }
}
