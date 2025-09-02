using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MedicalSystemApp.Data.Repositories;
using MedicalSystemApp.Models;
using MedicalSystemApp.Models.DTO;
using MedicalSystemApp.Extensions;

namespace MedicalSystemApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PovijestiBolestiController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        public PovijestiBolestiController(IUnitOfWork uow) => _uow = uow;

        // GET /api/povijestibolesti  -> sve stavke
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<PovijestBolestiDTO>>>> GetAll()
        {
            var list = await _uow.Repository<PovijestBolesti>().Query()
                .OrderByDescending(x => x.DatumPocetka)
                .ToListAsync();

            var dto = list.Select(x => new PovijestBolestiDTO
            {
                PovijestId = x.PovijestId,
                PacijentId = x.PacijentId,
                NazivBolesti = x.NazivBolesti,
                DatumPocetka = x.DatumPocetka.ToHrDate(),
                DatumZavrsetka = x.DatumZavrsetka?.ToHrDate()
            });

            return Ok(ApiResponse<IEnumerable<PovijestBolestiDTO>>.Ok(dto, "Povijesti bolesti dohvaćene."));
        }

        // GET /api/povijestibolesti/by-pacijent/5
        [HttpGet("by-pacijent/{pacijentId:int}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<PovijestBolestiDTO>>>> ByPacijent(int pacijentId)
        {
            var list = await _uow.Repository<PovijestBolesti>().Query()
                .Where(x => x.PacijentId == pacijentId)
                .OrderByDescending(x => x.DatumPocetka)
                .ToListAsync();

            var dto = list.Select(x => new PovijestBolestiDTO
            {
                PovijestId = x.PovijestId,
                PacijentId = x.PacijentId,
                NazivBolesti = x.NazivBolesti,
                DatumPocetka = x.DatumPocetka.ToHrDate(),
                DatumZavrsetka = x.DatumZavrsetka?.ToHrDate()
            });

            return Ok(ApiResponse<IEnumerable<PovijestBolestiDTO>>.Ok(dto, "Povijest bolesti za pacijenta dohvaćena."));
        }

        // GET /api/povijestibolesti/10
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<PovijestBolestiDTO>>> Get(int id)
        {
            var x = await _uow.Repository<PovijestBolesti>().GetByIdAsync(id);
            if (x == null) return NotFound(ApiResponse<PovijestBolestiDTO>.Fail("Zapis nije pronađen."));

            var dto = new PovijestBolestiDTO
            {
                PovijestId = x.PovijestId,
                PacijentId = x.PacijentId,
                NazivBolesti = x.NazivBolesti,
                DatumPocetka = x.DatumPocetka.ToHrDate(),
                DatumZavrsetka = x.DatumZavrsetka?.ToHrDate()
            };

            return Ok(ApiResponse<PovijestBolestiDTO>.Ok(dto, "Zapis dohvaćen."));
        }

        // POST /api/povijestibolesti
        [HttpPost]
        public async Task<ActionResult<ApiResponse<object>>> Create([FromBody] PovijestBolestiCreateDTO dto)
        {
            // provjeri FK: pacijent
            var pacijent = await _uow.Repository<Pacijent>().GetByIdAsync(dto.PacijentId);
            if (pacijent == null)
                return BadRequest(ApiResponse<object>.Fail("Pacijent s navedenim ID-om ne postoji."));

            var ent = new PovijestBolesti
            {
                PacijentId = dto.PacijentId,
                NazivBolesti = dto.NazivBolesti,
                DatumPocetka = dto.DatumPocetka.AsUtcDate(),
                DatumZavrsetka = dto.DatumZavrsetka?.AsUtcDate()
            };

            try
            {
                await _uow.Repository<PovijestBolesti>().AddAsync(ent);
                await _uow.SaveChangesAsync();
                return CreatedAtAction(nameof(Get), new { id = ent.PovijestId },
                    ApiResponse<object>.Ok(null, "Zapis povijesti bolesti kreiran."));
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail("Greška pri spremanju zapisa."));
            }
        }

        // PUT /api/povijestibolesti/10
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse<object>>> Update(int id, [FromBody] PovijestBolestiCreateDTO dto)
        {
            var repo = _uow.Repository<PovijestBolesti>();
            var ent = await repo.GetByIdAsync(id);
            if (ent == null) return NotFound(ApiResponse<object>.Fail("Zapis nije pronađen."));

            // provjeri FK: pacijent
            var pacijent = await _uow.Repository<Pacijent>().GetByIdAsync(dto.PacijentId);
            if (pacijent == null)
                return BadRequest(ApiResponse<object>.Fail("Pacijent s navedenim ID-om ne postoji."));

            ent.PacijentId = dto.PacijentId;
            ent.NazivBolesti = dto.NazivBolesti;
            ent.DatumPocetka = dto.DatumPocetka.AsUtcDate();
            ent.DatumZavrsetka = dto.DatumZavrsetka?.AsUtcDate();

            try
            {
                repo.Update(ent);
                await _uow.SaveChangesAsync();
                return Ok(ApiResponse<object>.Ok(null, "Zapis ažuriran."));
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail("Greška pri ažuriranju zapisa."));
            }
        }

        // DELETE /api/povijestibolesti/10
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
        {
            var repo = _uow.Repository<PovijestBolesti>();
            var ent = await repo.GetByIdAsync(id);
            if (ent == null) return NotFound(ApiResponse<object>.Fail("Zapis nije pronađen."));

            try
            {
                repo.Remove(ent);
                await _uow.SaveChangesAsync();
                return Ok(ApiResponse<object>.Ok(null, "Zapis izbrisan."));
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail("Brisanje nije uspjelo zbog greške na poslužitelju."));
            }
        }
    }
}
