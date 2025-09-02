namespace MedicalSystemApp.Models.DTO
{
    public class PregledDTO
    {
        public int PregledId { get; set; }
        public int PacijentId { get; set; }
        public string VrstaPregleda { get; set; } = "";
        public string DatumPregleda { get; set; } = ""; 
        public string? Doktor { get; set; }
    }
}
