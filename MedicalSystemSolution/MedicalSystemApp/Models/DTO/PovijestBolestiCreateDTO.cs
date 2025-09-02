namespace MedicalSystemApp.Models.DTO
{
    public class PovijestBolestiCreateDTO
    {
        public int PacijentId { get; set; }
        public string NazivBolesti { get; set; } = "";
        public DateTime DatumPocetka { get; set; }
        public DateTime? DatumZavrsetka { get; set; }
    }
}
