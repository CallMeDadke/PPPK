using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using MedicalSystemApp.Validation;

namespace MedicalSystemApp.Models
{
    [Index(nameof(OIB), IsUnique = true)]
    public class Pacijent
    {
        [Key]
        public int PacijentId { get; set; }

        [Required, MaxLength(100)]
        public string Ime { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Prezime { get; set; } = string.Empty;

        [Required]
        [StringLength(11)]
        [Oib]
        [Column(TypeName = "char(11)")]
        public string OIB { get; set; } = string.Empty;

        [Required]
        public DateTime DatumRodenja { get; set; }

        [Required, MaxLength(10)]
        [RegularExpression("Muško|Žensko")]
        public string Spol { get; set; } = "Muško";

        public virtual ICollection<PovijestBolesti> PovijestiBolesti { get; set; } = new List<PovijestBolesti>();
        public virtual ICollection<Pregled> Pregledi { get; set; } = new List<Pregled>();
        public virtual ICollection<Recept> Recepti { get; set; } = new List<Recept>();
    }
}
