using Microsoft.EntityFrameworkCore;
using MedicalSystemApp.Models;

namespace MedicalSystemApp.Data
{
    public class MedicalContext : DbContext
    {
        public MedicalContext(DbContextOptions<MedicalContext> options) : base(options) { }

        public virtual DbSet<Pacijent> Pacijenti { get; set; }
        public virtual DbSet<Doktor> Doktori { get; set; }
        public virtual DbSet<PovijestBolesti> PovijestiBolesti { get; set; }
        public virtual DbSet<VrstaPregleda> VrstePregleda { get; set; }
        public virtual DbSet<Pregled> Pregledi { get; set; }
        public virtual DbSet<Dokument> Dokumenti { get; set; }
        public virtual DbSet<Lijek> Lijekovi { get; set; }
        public virtual DbSet<Recept> Recepti { get; set; }
        public virtual DbSet<StavkaRecepta> StavkeRecepata { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VrstaPregleda>().HasData(
                new VrstaPregleda { VrstaPregledaId = 1, Sifra = "GP", Naziv = "Opći pregled" },
                new VrstaPregleda { VrstaPregledaId = 2, Sifra = "KRV", Naziv = "Test krvi" },
                new VrstaPregleda { VrstaPregledaId = 3, Sifra = "X-RAY", Naziv = "Rendgen" },
                new VrstaPregleda { VrstaPregledaId = 4, Sifra = "CT", Naziv = "CT sken" },
                new VrstaPregleda { VrstaPregledaId = 5, Sifra = "MR", Naziv = "MRI sken" },
                new VrstaPregleda { VrstaPregledaId = 6, Sifra = "ULTRA", Naziv = "Ultrazvuk" },
                new VrstaPregleda { VrstaPregledaId = 7, Sifra = "EKG", Naziv = "Elektrokardiogram" },
                new VrstaPregleda { VrstaPregledaId = 8, Sifra = "ECHO", Naziv = "Ehokardiogram" },
                new VrstaPregleda { VrstaPregledaId = 9, Sifra = "EYE", Naziv = "Pregled očiju" },
                new VrstaPregleda { VrstaPregledaId = 10, Sifra = "DERM", Naziv = "Dermatološki pregled" },
                new VrstaPregleda { VrstaPregledaId = 11, Sifra = "DENTA", Naziv = "Pregled zuba" },
                new VrstaPregleda { VrstaPregledaId = 12, Sifra = "MAMMO", Naziv = "Mamografija" },
                new VrstaPregleda { VrstaPregledaId = 13, Sifra = "NEURO", Naziv = "Neurološki pregled" }
            );
        }
    }
}
