using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalSystemApp.Models
{
    public class Pregled
    {
        [Key]
        public int PregledId { get; set; }

        [Required]
        public int PacijentId { get; set; }

        [ForeignKey(nameof(PacijentId))]
        public virtual Pacijent Pacijent { get; set; } = null!;

        public int? DoktorId { get; set; }

        [ForeignKey(nameof(DoktorId))]
        public virtual Doktor? Doktor { get; set; }

        [Required]
        public int VrstaPregledaId { get; set; }

        [ForeignKey(nameof(VrstaPregledaId))]
        public virtual VrstaPregleda VrstaPregleda { get; set; } = null!;

        [Required]
        public DateTime DatumPregleda { get; set; }

        public virtual ICollection<Dokument> Dokumenti { get; set; } = new List<Dokument>();
    }
}
