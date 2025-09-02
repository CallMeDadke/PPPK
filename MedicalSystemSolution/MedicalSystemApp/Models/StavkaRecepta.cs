using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalSystemApp.Models
{
    public class StavkaRecepta
    {
        [Key]
        public int StavkaReceptaId { get; set; }

        [Required]
        public int ReceptId { get; set; }

        [ForeignKey(nameof(ReceptId))]
        public virtual Recept Recept { get; set; } = null!;

        [Required]
        public int LijekId { get; set; }

        [ForeignKey(nameof(LijekId))]
        public virtual Lijek Lijek { get; set; } = null!;

        [Required, MaxLength(100)]
        public string Doziranje { get; set; } = string.Empty;
    }
}
