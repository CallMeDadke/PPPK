namespace MedicalSystemApp.Models.DTO
{
    public class ReceptCreateDTO
    {
        public int PacijentId { get; set; }
        public DateTime DatumIzdavanja { get; set; }
        public List<StavkaReceptaCreateDTO> Stavke { get; set; } = new();
    }
}
