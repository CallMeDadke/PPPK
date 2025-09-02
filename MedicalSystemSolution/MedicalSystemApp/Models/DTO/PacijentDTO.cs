namespace MedicalSystemApp.Models.DTO
{
    public class PacijentDTO
    {
        public int PacijentId { get; set; }
        public string Ime { get; set; } = string.Empty;
        public string Prezime { get; set; } = string.Empty;
        public string OIB { get; set; } = string.Empty;
        public string DatumRodenja { get; set; } = string.Empty; 
        public string Spol { get; set; } = string.Empty;
    }
}
