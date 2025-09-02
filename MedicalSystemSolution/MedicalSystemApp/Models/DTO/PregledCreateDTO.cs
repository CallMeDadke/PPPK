namespace MedicalSystemApp.Models.DTO
{
    public class PregledCreateDTO
    {
        public int PacijentId { get; set; }
        public int VrstaPregledaId { get; set; }
        public DateTime DatumPregleda { get; set; } 
        public int? DoktorId { get; set; }
    }
}
