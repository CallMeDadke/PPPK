using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace MedicalSystemApp.Models
{
    [Index(nameof(Sifra), IsUnique = true)]
    public class VrstaPregleda
    {
        [Key]
        public int VrstaPregledaId { get; set; }

        [Required, MaxLength(10)]
        public string Sifra { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Naziv { get; set; } = string.Empty;
    }
}
