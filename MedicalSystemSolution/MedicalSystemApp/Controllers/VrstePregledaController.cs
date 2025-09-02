
using Microsoft.AspNetCore.Mvc;
using MedicalSystemApp.Models.DTO;

namespace MedicalSystemApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VrstePregledaController : ControllerBase
    {
        private static readonly VrstaPregledaDTO[] _data = new[]
        {
            new VrstaPregledaDTO { Sifra="GP",    Naziv="Opći tjelesni pregled" },
            new VrstaPregledaDTO { Sifra="KRV",   Naziv="Test krvi" },
            new VrstaPregledaDTO { Sifra="X-RAY", Naziv="Rendgensko skeniranje" },
            new VrstaPregledaDTO { Sifra="CT",    Naziv="CT sken" },
            new VrstaPregledaDTO { Sifra="MR",    Naziv="MRI sken" },
            new VrstaPregledaDTO { Sifra="ULTRA", Naziv="Ultrazvuk" },
            new VrstaPregledaDTO { Sifra="EKG",   Naziv="Elektrokardiogram" },
            new VrstaPregledaDTO { Sifra="ECHO",  Naziv="Ehokardiogram" },
            new VrstaPregledaDTO { Sifra="EYE",   Naziv="Pregled očiju" },
            new VrstaPregledaDTO { Sifra="DERM",  Naziv="Dermatološki pregled" },
            new VrstaPregledaDTO { Sifra="DENTA", Naziv="Pregled zuba" },
            new VrstaPregledaDTO { Sifra="MAMMO", Naziv="Mamografija" },
            new VrstaPregledaDTO { Sifra="NEURO", Naziv="Neurološki pregled" }
        };

        [HttpGet]
        public ActionResult<ApiResponse<IEnumerable<VrstaPregledaDTO>>> Get()
            => Ok(ApiResponse<IEnumerable<VrstaPregledaDTO>>.Ok(_data, "Šife za vrste pregleda."));
    }
}
