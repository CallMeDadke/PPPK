namespace MedicalSystemApp.Models.DTO
{
    public class StavkaReceptaDTO
    {
        public int StavkaReceptaId { get; set; }
        public int LijekId { get; set; }
        public string Lijek { get; set; } = "";
        public string Doziranje { get; set; } = "";
    }
}
