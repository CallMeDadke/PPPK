namespace MedicalSystemApp.Models.DTO
{
    public class DokumentDTO
    {
        public int DokumentId { get; set; }
        public int PregledId { get; set; }
        public string NazivDatoteke { get; set; } = "";
        public string Putanja { get; set; } = "";   
        public string? TipDokumenta { get; set; }
        public string DatumUnosa { get; set; } = "";
    }
}
