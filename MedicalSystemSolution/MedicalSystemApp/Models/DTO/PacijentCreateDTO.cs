using System.ComponentModel.DataAnnotations;

namespace MedicalSystemApp.Models.DTO
{
    public class PacijentCreateDTO
    {
        [Required, MaxLength(100)]
        public string Ime { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Prezime { get; set; } = string.Empty;

        [Required, StringLength(11)]
        public string OIB { get; set; } = string.Empty;

        [Required]
        public DateTime DatumRodenja { get; set; } 
        [Required, RegularExpression("Muško|Žensko")]
        public string Spol { get; set; } = "Muško";
    }
}
