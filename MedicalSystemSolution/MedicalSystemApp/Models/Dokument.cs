using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalSystemApp.Models
{
    public class Dokument
    {
        [Key]
        public int DokumentId { get; set; }

        [Required]
        public int PregledId { get; set; }

        [ForeignKey(nameof(PregledId))]
        public virtual Pregled Pregled { get; set; } = null!;

        [Required, MaxLength(200)]
        public string NazivDatoteke { get; set; } = string.Empty;

        [Required]
        public string Putanja { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? TipDokumenta { get; set; }

        public DateTime DatumUnosa { get; set; } = DateTime.UtcNow;
    }
}
