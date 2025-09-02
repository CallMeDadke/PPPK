namespace MedicalSystemApp.Models.DTO
{
    public class ReceptDTO
    {
        public int ReceptId { get; set; }
        public int PacijentId { get; set; }
        public string DatumIzdavanja { get; set; } = "";
        public List<StavkaReceptaDTO> Stavke { get; set; } = new();
    }
}
