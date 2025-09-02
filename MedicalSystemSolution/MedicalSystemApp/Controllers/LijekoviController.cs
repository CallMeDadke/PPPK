using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MedicalSystemApp.Data.Repositories;
using MedicalSystemApp.Models;
using MedicalSystemApp.Models.DTO;

namespace MedicalSystemApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LijekoviController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        public LijekoviController(IUnitOfWork uow) => _uow = uow;

        // GET /api/lijekovi  -> svi (po nazivu)
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<LijekDTO>>>> GetAll()
        {
            var list = await _uow.Repository<Lijek>().Query()
                .OrderBy(x => x.Naziv)
                .ToListAsync();

            var dto = list.Select(x => new LijekDTO { LijekId = x.LijekId, Naziv = x.Naziv });
            return Ok(ApiResponse<IEnumerable<LijekDTO>>.Ok(dto, "Lijekovi dohvaćeni."));
        }

        // GET /api/lijekovi/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<LijekDTO>>> Get(int id)
        {
            var ent = await _uow.Repository<Lijek>().GetByIdAsync(id);
            if (ent == null) return NotFound(ApiResponse<LijekDTO>.Fail("Lijek nije pronađen."));
            var dto = new LijekDTO { LijekId = ent.LijekId, Naziv = ent.Naziv };
            return Ok(ApiResponse<LijekDTO>.Ok(dto, "Lijek dohvaćen."));
        }

        // GET /api/lijekovi/search?naziv=par
        [HttpGet("search")]
        public async Task<ActionResult<ApiResponse<IEnumerable<LijekDTO>>>> Search([FromQuery] string naziv)
        {
            var q = _uow.Repository<Lijek>().Query();
            if (!string.IsNullOrWhiteSpace(naziv))
                q = q.Where(x => x.Naziv.ToLower().Contains(naziv.ToLower()));

            var list = await q.OrderBy(x => x.Naziv).ToListAsync();
            var dto = list.Select(x => new LijekDTO { LijekId = x.LijekId, Naziv = x.Naziv });
            return Ok(ApiResponse<IEnumerable<LijekDTO>>.Ok(dto, "Rezultati pretraživanja."));
        }

        // POST /api/lijekovi
        [HttpPost]
        public async Task<ActionResult<ApiResponse<object>>> Create([FromBody] LijekCreateDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Naziv))
                return BadRequest(ApiResponse<object>.Fail("Naziv lijeka je obavezan."));

            // jednostavna provjera duplikata po nazivu
            var exists = await _uow.Repository<Lijek>().Query()
                .AnyAsync(x => x.Naziv.ToLower() == dto.Naziv.ToLower());
            if (exists) return Conflict(ApiResponse<object>.Fail("Lijek s tim nazivom već postoji."));

            var ent = new Lijek { Naziv = dto.Naziv, Opis = dto.Opis };
            try
            {
                await _uow.Repository<Lijek>().AddAsync(ent);
                await _uow.SaveChangesAsync();
                return CreatedAtAction(nameof(Get), new { id = ent.LijekId },
                    ApiResponse<object>.Ok(null, "Lijek kreiran."));
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail("Greška pri spremanju lijeka."));
            }
        }

        // PUT /api/lijekovi/5
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse<object>>> Update(int id, [FromBody] LijekCreateDTO dto)
        {
            var repo = _uow.Repository<Lijek>();
            var ent = await repo.GetByIdAsync(id);
            if (ent == null) return NotFound(ApiResponse<object>.Fail("Lijek nije pronađen."));

            if (string.IsNullOrWhiteSpace(dto.Naziv))
                return BadRequest(ApiResponse<object>.Fail("Naziv lijeka je obavezan."));

            // provjera duplikata (drugi zapis sa istim nazivom)
            var duplicate = await repo.Query()
                .AnyAsync(x => x.LijekId != id && x.Naziv.ToLower() == dto.Naziv.ToLower());
            if (duplicate) return Conflict(ApiResponse<object>.Fail("Lijek s tim nazivom već postoji."));

            ent.Naziv = dto.Naziv;
            ent.Opis = dto.Opis;

            try
            {
                repo.Update(ent);
                await _uow.SaveChangesAsync();
                return Ok(ApiResponse<object>.Ok(null, "Lijek ažuriran."));
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail("Greška pri ažuriranju lijeka."));
            }
        }

        // DELETE /api/lijekovi/5
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
        {
            var repo = _uow.Repository<Lijek>();
            var ent = await repo.GetByIdAsync(id);
            if (ent == null) return NotFound(ApiResponse<object>.Fail("Lijek nije pronađen."));

            try
            {
                repo.Remove(ent);
                await _uow.SaveChangesAsync();
                return Ok(ApiResponse<object>.Ok(null, "Lijek izbrisan."));
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail("Brisanje nije uspjelo zbog greške na poslužitelju."));
            }
        }
    }
}
