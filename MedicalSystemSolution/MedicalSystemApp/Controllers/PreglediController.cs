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
    public class PreglediController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        public PreglediController(IUnitOfWork uow) => _uow = uow;

        // GET /api/pregledi  -> svi pregledi
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<PregledDTO>>>> GetAll()
        {
            var list = await _uow.Repository<Pregled>().Query()
                .Include(x => x.VrstaPregleda)
                .Include(x => x.Doktor)
                .OrderBy(x => x.DatumPregleda)
                .ToListAsync();

            var dto = list.Select(x => new PregledDTO
            {
                PregledId = x.PregledId,
                PacijentId = x.PacijentId,
                VrstaPregleda = x.VrstaPregleda.Sifra,
                DatumPregleda = x.DatumPregleda.ToHrDate(),
                Doktor = x.Doktor == null ? null : $"{x.Doktor.Ime} {x.Doktor.Prezime}"
            });

            return Ok(ApiResponse<IEnumerable<PregledDTO>>.Ok(dto, "Pregledi dohvaćeni."));
        }

        // GET /api/pregledi/by-pacijent/5  -> pregledi za pacijenta
        [HttpGet("by-pacijent/{pacijentId:int}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<PregledDTO>>>> GetByPacijent(int pacijentId)
        {
            var list = await _uow.Repository<Pregled>().Query()
                .Where(x => x.PacijentId == pacijentId)
                .Include(x => x.VrstaPregleda)
                .Include(x => x.Doktor)
                .OrderBy(x => x.DatumPregleda)
                .ToListAsync();

            var dto = list.Select(x => new PregledDTO
            {
                PregledId = x.PregledId,
                PacijentId = x.PacijentId,
                VrstaPregleda = x.VrstaPregleda.Sifra,
                DatumPregleda = x.DatumPregleda.ToHrDate(),
                Doktor = x.Doktor == null ? null : $"{x.Doktor.Ime} {x.Doktor.Prezime}"
            });

            return Ok(ApiResponse<IEnumerable<PregledDTO>>.Ok(dto, "Pregledi pacijenta dohvaćeni."));
        }

        // GET /api/pregledi/10  -> detalj pregleda
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<PregledDTO>>> Get(int id)
        {
            var p = await _uow.Repository<Pregled>().Query()
                .Include(x => x.VrstaPregleda)
                .Include(x => x.Doktor)
                .FirstOrDefaultAsync(x => x.PregledId == id);

            if (p == null) return NotFound(ApiResponse<PregledDTO>.Fail("Pregled nije pronađen."));

            var dto = new PregledDTO
            {
                PregledId = p.PregledId,
                PacijentId = p.PacijentId,
                VrstaPregleda = p.VrstaPregleda.Sifra,
                DatumPregleda = p.DatumPregleda.ToHrDate(),
                Doktor = p.Doktor == null ? null : $"{p.Doktor.Ime} {p.Doktor.Prezime}"
            };

            return Ok(ApiResponse<PregledDTO>.Ok(dto, "Pregled dohvaćen."));
        }

        // POST /api/pregledi  -> kreiranje pregleda
        [HttpPost]
        public async Task<ActionResult<ApiResponse<object>>> Create([FromBody] PregledCreateDTO dto)
        {
            // VALIDACIJE prije inserta (da ne potrošimo ID)
            var pacijent = await _uow.Repository<Pacijent>().GetByIdAsync(dto.PacijentId);
            if (pacijent == null)
                return BadRequest(ApiResponse<object>.Fail("Pacijent s navedenim ID-om ne postoji."));

            var vrsta = await _uow.Repository<VrstaPregleda>().GetByIdAsync(dto.VrstaPregledaId);
            if (vrsta == null)
                return BadRequest(ApiResponse<object>.Fail("Vrsta pregleda s navedenim ID-om ne postoji."));

            if (dto.DoktorId.HasValue)
            {
                var doktor = await _uow.Repository<Doktor>().GetByIdAsync(dto.DoktorId.Value);
                if (doktor == null)
                    return BadRequest(ApiResponse<object>.Fail("Doktor s navedenim ID-om ne postoji."));
            }

            var ent = new Pregled
            {
                PacijentId = dto.PacijentId,
                VrstaPregledaId = dto.VrstaPregledaId,
                DoktorId = dto.DoktorId,
                DatumPregleda = dto.DatumPregleda.AsUtcDate()
            };

            try
            {
                await _uow.Repository<Pregled>().AddAsync(ent);
                await _uow.SaveChangesAsync();
                return CreatedAtAction(nameof(Get), new { id = ent.PregledId },
                    ApiResponse<object>.Ok(null, "Pregled uspješno kreiran."));
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail("Došlo je do greške pri spremanju pregleda."));
            }
        }

        // PUT /api/pregledi/10  -> ažuriranje pregleda
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse<object>>> Update(int id, [FromBody] PregledCreateDTO dto)
        {
            var repo = _uow.Repository<Pregled>();
            var ent = await repo.GetByIdAsync(id);
            if (ent == null) return NotFound(ApiResponse<object>.Fail("Pregled nije pronađen."));

            // VALIDACIJE FK-ova
            var pacijent = await _uow.Repository<Pacijent>().GetByIdAsync(dto.PacijentId);
            if (pacijent == null)
                return BadRequest(ApiResponse<object>.Fail("Pacijent s navedenim ID-om ne postoji."));

            var vrsta = await _uow.Repository<VrstaPregleda>().GetByIdAsync(dto.VrstaPregledaId);
            if (vrsta == null)
                return BadRequest(ApiResponse<object>.Fail("Vrsta pregleda s navedenim ID-om ne postoji."));

            if (dto.DoktorId.HasValue)
            {
                var doktor = await _uow.Repository<Doktor>().GetByIdAsync(dto.DoktorId.Value);
                if (doktor == null)
                    return BadRequest(ApiResponse<object>.Fail("Doktor s navedenim ID-om ne postoji."));
            }

            ent.PacijentId = dto.PacijentId;
            ent.VrstaPregledaId = dto.VrstaPregledaId;
            ent.DoktorId = dto.DoktorId;
            ent.DatumPregleda = dto.DatumPregleda.AsUtcDate();

            try
            {
                repo.Update(ent);
                await _uow.SaveChangesAsync();
                return Ok(ApiResponse<object>.Ok(null, "Pregled uspješno ažuriran."));
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail("Došlo je do greške pri ažuriranju pregleda."));
            }
        }

        // DELETE /api/pregledi/10
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
        {
            var repo = _uow.Repository<Pregled>();
            var ent = await repo.GetByIdAsync(id);
            if (ent == null) return NotFound(ApiResponse<object>.Fail("Pregled nije pronađen."));

            try
            {
                repo.Remove(ent);
                await _uow.SaveChangesAsync();
                return Ok(ApiResponse<object>.Ok(null, "Pregled uspješno izbrisan."));
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail("Brisanje nije uspjelo zbog greške na poslužitelju."));
            }
        }
    }
}
