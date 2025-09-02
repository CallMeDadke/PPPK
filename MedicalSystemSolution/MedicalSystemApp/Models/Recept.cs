using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalSystemApp.Models
{
    public class Recept
    {
        [Key]
        public int ReceptId { get; set; }

        [Required]
        public int PacijentId { get; set; }

        [ForeignKey(nameof(PacijentId))]
        public virtual Pacijent Pacijent { get; set; } = null!;

        [Required]
        public DateTime DatumIzdavanja { get; set; }

        public virtual ICollection<StavkaRecepta> Stavke { get; set; } = new List<StavkaRecepta>();
    }
}
