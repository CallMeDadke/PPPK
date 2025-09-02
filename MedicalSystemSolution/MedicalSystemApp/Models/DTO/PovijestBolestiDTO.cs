namespace MedicalSystemApp.Models.DTO
{
    public class PovijestBolestiDTO
    {
        public int PovijestId { get; set; }
        public int PacijentId { get; set; }
        public string NazivBolesti { get; set; } = "";
        public string DatumPocetka { get; set; } = "";
        public string? DatumZavrsetka { get; set; }
    }
}
