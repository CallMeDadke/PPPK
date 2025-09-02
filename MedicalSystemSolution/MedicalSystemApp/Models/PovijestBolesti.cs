using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalSystemApp.Models
{
    public class PovijestBolesti
    {
        [Key]
        public int PovijestId { get; set; }

        [Required]
        public int PacijentId { get; set; }

        [ForeignKey(nameof(PacijentId))]
        public virtual Pacijent Pacijent { get; set; } = null!;

        [Required, MaxLength(200)]
        public string NazivBolesti { get; set; } = string.Empty;

        [Required]
        public DateTime DatumPocetka { get; set; }

        public DateTime? DatumZavrsetka { get; set; }
    }
}
