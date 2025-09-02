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
    public class ReceptiController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        public ReceptiController(IUnitOfWork uow) => _uow = uow;

        // GET /api/recepti
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<ReceptDTO>>>> GetAll()
        {
            var list = await _uow.Repository<Recept>().Query()
                .Include(r => r.Stavke).ThenInclude(s => s.Lijek)
                .OrderByDescending(x => x.DatumIzdavanja)
                .ToListAsync();

            var dto = list.Select(r => new ReceptDTO
            {
                ReceptId = r.ReceptId,
                PacijentId = r.PacijentId,
                DatumIzdavanja = r.DatumIzdavanja.ToHrDate(),
                Stavke = r.Stavke.Select(s => new StavkaReceptaDTO
                {
                    StavkaReceptaId = s.StavkaReceptaId,
                    LijekId = s.LijekId,
                    Lijek = s.Lijek.Naziv,
                    Doziranje = s.Doziranje
                }).ToList()
            });

            return Ok(ApiResponse<IEnumerable<ReceptDTO>>.Ok(dto, "Recepti dohvaćeni."));
        }



        // GET /api/recepti/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<ReceptDTO>>> Get(int id)
        {
            var r = await _uow.Repository<Recept>().Query()
                .Include(x => x.Stavke).ThenInclude(s => s.Lijek)
                .FirstOrDefaultAsync(x => x.ReceptId == id);

            if (r == null) return NotFound(ApiResponse<ReceptDTO>.Fail("Recept nije pronađen."));

            var dto = new ReceptDTO
            {
                ReceptId = r.ReceptId,
                PacijentId = r.PacijentId,
                DatumIzdavanja = r.DatumIzdavanja.ToHrDate(),
                Stavke = r.Stavke.Select(s => new StavkaReceptaDTO
                {
                    StavkaReceptaId = s.StavkaReceptaId,
                    LijekId = s.LijekId,
                    Lijek = s.Lijek.Naziv,
                    Doziranje = s.Doziranje
                }).ToList()
            };

            return Ok(ApiResponse<ReceptDTO>.Ok(dto, "Recept dohvaćen."));
        }

        // POST /api/recepti
        [HttpPost]
        public async Task<ActionResult<ApiResponse<object>>> Create([FromBody] ReceptCreateDTO dto)
        {
            // validacija FK: pacijent
            var pacijent = await _uow.Repository<Pacijent>().GetByIdAsync(dto.PacijentId);
            if (pacijent == null)
                return BadRequest(ApiResponse<object>.Fail("Pacijent s navedenim ID-om ne postoji."));

            // validacija lijekova
            foreach (var s in dto.Stavke)
            {
                var lijek = await _uow.Repository<Lijek>().GetByIdAsync(s.LijekId);
                if (lijek == null)
                    return BadRequest(ApiResponse<object>.Fail($"Lijek s ID={s.LijekId} ne postoji."));
            }

            var ent = new Recept
            {
                PacijentId = dto.PacijentId,
                DatumIzdavanja = dto.DatumIzdavanja.AsUtcDate(),
                Stavke = dto.Stavke.Select(s => new StavkaRecepta
                {
                    LijekId = s.LijekId,
                    Doziranje = s.Doziranje
                }).ToList()
            };

            try
            {
                await _uow.Repository<Recept>().AddAsync(ent);
                await _uow.SaveChangesAsync();
                return CreatedAtAction(nameof(Get), new { id = ent.ReceptId },
                    ApiResponse<object>.Ok(null, "Recept kreiran."));
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail("Greška pri spremanju recepta."));
            }
        }

        [HttpGet("by-pacijent/{pacijentId:int}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ReceptDTO>>>> GetByPacijent(int pacijentId)
        {
            var list = await _uow.Repository<Recept>().Query()
                .Where(x => x.PacijentId == pacijentId)
                .Include(r => r.Stavke).ThenInclude(s => s.Lijek)
                .OrderByDescending(x => x.DatumIzdavanja)
                .ToListAsync();

            var dto = list.Select(r => new ReceptDTO
            {
                ReceptId = r.ReceptId,
                PacijentId = r.PacijentId,
                DatumIzdavanja = r.DatumIzdavanja.ToHrDate(),
                Stavke = r.Stavke.Select(s => new StavkaReceptaDTO
                {
                    StavkaReceptaId = s.StavkaReceptaId,
                    LijekId = s.LijekId,
                    Lijek = s.Lijek.Naziv,
                    Doziranje = s.Doziranje
                }).ToList()
            });

            return Ok(ApiResponse<IEnumerable<ReceptDTO>>.Ok(dto, "Recepti pacijenta dohvaćeni."));
        }


        // PUT /api/recepti/5
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse<object>>> Update(int id, [FromBody] ReceptCreateDTO dto)
        {
            var repo = _uow.Repository<Recept>();
            var ent = await repo.Query().Include(r => r.Stavke).FirstOrDefaultAsync(r => r.ReceptId == id);
            if (ent == null) return NotFound(ApiResponse<object>.Fail("Recept nije pronađen."));

            // validacija FK: pacijent
            var pacijent = await _uow.Repository<Pacijent>().GetByIdAsync(dto.PacijentId);
            if (pacijent == null)
                return BadRequest(ApiResponse<object>.Fail("Pacijent s navedenim ID-om ne postoji."));

            // validacija lijekova
            foreach (var s in dto.Stavke)
            {
                var lijek = await _uow.Repository<Lijek>().GetByIdAsync(s.LijekId);
                if (lijek == null)
                    return BadRequest(ApiResponse<object>.Fail($"Lijek s ID={s.LijekId} ne postoji."));
            }

            ent.PacijentId = dto.PacijentId;
            ent.DatumIzdavanja = dto.DatumIzdavanja.AsUtcDate();

            // najlakše: isprazni i ponovno napuni stavke
            ent.Stavke.Clear();
            foreach (var s in dto.Stavke)
                ent.Stavke.Add(new StavkaRecepta { LijekId = s.LijekId, Doziranje = s.Doziranje });

            try
            {
                repo.Update(ent);
                await _uow.SaveChangesAsync();
                return Ok(ApiResponse<object>.Ok(null, "Recept ažuriran."));
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail("Greška pri ažuriranju recepta."));
            }
        }

        // DELETE /api/recepti/5
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
        {
            var repo = _uow.Repository<Recept>();
            var ent = await repo.GetByIdAsync(id);
            if (ent == null) return NotFound(ApiResponse<object>.Fail("Recept nije pronađen."));

            try
            {
                repo.Remove(ent);
                await _uow.SaveChangesAsync();
                return Ok(ApiResponse<object>.Ok(null, "Recept izbrisan."));
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail("Brisanje recepta nije uspjelo."));
            }
        }
    }
}
