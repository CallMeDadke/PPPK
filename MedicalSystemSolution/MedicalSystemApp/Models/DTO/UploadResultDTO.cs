namespace MedicalSystemApp.Models.DTO
{
    public class UploadResultDTO
    {
        public int DokumentId { get; set; }
        public string Poruka { get; set; } = "";
        public string? Url { get; set; }
    }
}
