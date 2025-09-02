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
    public class PacijentiController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        public PacijentiController(IUnitOfWork uow) => _uow = uow;

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<PacijentDTO>>>> GetAll()
        {
            var list = await _uow.Repository<Pacijent>().Query()
                .OrderBy(p => p.Prezime).ThenBy(p => p.Ime)
                .ToListAsync();

            var dto = list.Select(p => new PacijentDTO
            {
                PacijentId = p.PacijentId,
                Ime = p.Ime,
                Prezime = p.Prezime,
                OIB = p.OIB,
                DatumRodenja = p.DatumRodenja.ToHrDate(),
                Spol = p.Spol
            });

            return Ok(ApiResponse<IEnumerable<PacijentDTO>>.Ok(dto, "Popis pacijenata dohvaćen."));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<PacijentDTO>>> Get(int id)
        {
            var p = await _uow.Repository<Pacijent>().GetByIdAsync(id);
            if (p == null) return NotFound(ApiResponse<PacijentDTO>.Fail("Pacijent nije pronađen."));

            var dto = new PacijentDTO
            {
                PacijentId = p.PacijentId,
                Ime = p.Ime,
                Prezime = p.Prezime,
                OIB = p.OIB,
                DatumRodenja = p.DatumRodenja.ToHrDate(),
                Spol = p.Spol
            };
            return Ok(ApiResponse<PacijentDTO>.Ok(dto, "Pacijent dohvaćen."));
        }

        [HttpGet("search")]
        public async Task<ActionResult<ApiResponse<IEnumerable<PacijentDTO>>>> Search([FromQuery] string? oib, [FromQuery] string? prezime)
        {
            var q = _uow.Repository<Pacijent>().Query();
            if (!string.IsNullOrWhiteSpace(oib)) q = q.Where(p => p.OIB == oib);
            if (!string.IsNullOrWhiteSpace(prezime)) q = q.Where(p => p.Prezime.ToLower().Contains(prezime.ToLower()));

            var list = await q.OrderBy(p => p.Prezime).ThenBy(p => p.Ime).ToListAsync();

            var dto = list.Select(p => new PacijentDTO
            {
                PacijentId = p.PacijentId,
                Ime = p.Ime,
                Prezime = p.Prezime,
                OIB = p.OIB,
                DatumRodenja = p.DatumRodenja.ToHrDate(),
                Spol = p.Spol
            });

            return Ok(ApiResponse<IEnumerable<PacijentDTO>>.Ok(dto, "Rezultati pretraživanja."));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<PacijentDTO>>> Create([FromBody] PacijentCreateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<PacijentDTO>.Fail("Neispravni podaci. Provjeri polja i pokušaj ponovno."));

            try
            {
                var ent = new Pacijent
                {
                    Ime = dto.Ime,
                    Prezime = dto.Prezime,
                    OIB = dto.OIB,
                    DatumRodenja = dto.DatumRodenja.AsUtcDate(),
                    Spol = dto.Spol
                };

                await _uow.Repository<Pacijent>().AddAsync(ent);
                await _uow.SaveChangesAsync();

                var outDto = new PacijentDTO
                {
                    PacijentId = ent.PacijentId,
                    Ime = ent.Ime,
                    Prezime = ent.Prezime,
                    OIB = ent.OIB,
                    DatumRodenja = ent.DatumRodenja.ToHrDate(),
                    Spol = ent.Spol
                };

                return CreatedAtAction(nameof(Get), new { id = ent.PacijentId },
                    ApiResponse<PacijentDTO>.Ok(outDto, "Pacijent uspješno kreiran."));
            }
            catch (DbUpdateException)
            {
                return Conflict(ApiResponse<PacijentDTO>.Fail("Sukob pri spremanju (možda već postoji isti OIB)."));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<PacijentDTO>.Fail("Došlo je do neočekivane greške."));
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse<object>>> Update(int id, [FromBody] PacijentCreateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("Neispravni podaci."));

            var repo = _uow.Repository<Pacijent>();
            var ent = await repo.GetByIdAsync(id);
            if (ent == null) return NotFound(ApiResponse<object>.Fail("Pacijent nije pronađen."));

            try
            {
                ent.Ime = dto.Ime;
                ent.Prezime = dto.Prezime;
                ent.OIB = dto.OIB;
                ent.DatumRodenja = dto.DatumRodenja.AsUtcDate();
                ent.Spol = dto.Spol;

                repo.Update(ent);
                await _uow.SaveChangesAsync();
                return Ok(ApiResponse<object>.Ok(null, "Pacijent uspješno ažuriran."));
            }
            catch (DbUpdateException)
            {
                return Conflict(ApiResponse<object>.Fail("Sukob pri spremanju (provjeri OIB)."));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail("Došlo je do neočekivane greške."));
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
        {
            var repo = _uow.Repository<Pacijent>();
            var ent = await repo.GetByIdAsync(id);
            if (ent == null) return NotFound(ApiResponse<object>.Fail("Pacijent nije pronađen."));

            try
            {
                repo.Remove(ent);
                await _uow.SaveChangesAsync();
                return Ok(ApiResponse<object>.Ok(null, "Pacijent uspješno izbrisan."));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail("Brisanje nije uspjelo zbog greške na poslužitelju."));
            }
        }

        [HttpGet("export/csv")]
        public async Task<FileContentResult> ExportCsv([FromServices] IUnitOfWork uow)
        {
            var list = await uow.Repository<Pacijent>().GetAllAsync();

            var sb = new System.Text.StringBuilder();
            // TAB kao razdjelnik (tsv stil) – Excel ga odlično prepoznaje
            sb.AppendLine("PacijentId\tIme\tPrezime\tOIB\tDatumRodenja\tSpol");

            foreach (var p in list)
                sb.AppendLine($"{p.PacijentId}\t{p.Ime}\t{p.Prezime}\t{p.OIB}\t{p.DatumRodenja:dd.MM.yyyy.}\t{p.Spol}");

            // UTF-16 LE + BOM (Excel friendly)
            var enc = System.Text.Encoding.Unicode; // UTF-16 LE
            var bytes = enc.GetPreamble().Concat(enc.GetBytes(sb.ToString())).ToArray();

            // Content-Type kao Excel (može i text/tab-separated-values)
            return File(bytes, "application/vnd.ms-excel", $"pacijenti_{DateTime.UtcNow:yyyyMMdd}.csv");
        }


    }
}
