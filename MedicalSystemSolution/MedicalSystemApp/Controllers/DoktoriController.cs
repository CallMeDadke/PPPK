using Microsoft.AspNetCore.Mvc;
using MedicalSystemApp.Data.Repositories;
using MedicalSystemApp.Models;
using MedicalSystemApp.Models.DTO;

namespace MedicalSystemApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoktoriController : ControllerBase
    {
        private readonly IUnitOfWork _uow;

        public DoktoriController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // GET: /api/doktori
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<DoktorDTO>>>> GetAll()
        {
            var list = await _uow.Repository<Doktor>().GetAllAsync();
            var dto = list.Select(d => new DoktorDTO
            {
                DoktorId = d.DoktorId,
                Ime = d.Ime,
                Prezime = d.Prezime
            });
            return Ok(ApiResponse<IEnumerable<DoktorDTO>>.Ok(dto, "Lista doktora dohvaćena."));
        }

        // GET: /api/doktori/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<DoktorDTO>>> GetById(int id)
        {
            var ent = await _uow.Repository<Doktor>().GetByIdAsync(id);
            if (ent == null)
                return NotFound(ApiResponse<DoktorDTO>.Fail("Doktor nije pronađen."));

            var dto = new DoktorDTO
            {
                DoktorId = ent.DoktorId,
                Ime = ent.Ime,
                Prezime = ent.Prezime
            };
            return Ok(ApiResponse<DoktorDTO>.Ok(dto, "Doktor dohvaćen."));
        }

        // POST: /api/doktori
        // Body: { "ime": "...", "prezime": "..." }
        [HttpPost]
        public async Task<ActionResult<ApiResponse<DoktorDTO>>> Create([FromBody] CreateDoktorDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Ime) || string.IsNullOrWhiteSpace(dto.Prezime))
                return BadRequest(ApiResponse<DoktorDTO>.Fail("Ime i prezime su obavezni."));

            var ent = new Doktor
            {
                Ime = dto.Ime.Trim(),
                Prezime = dto.Prezime.Trim()
            };

            await _uow.Repository<Doktor>().AddAsync(ent);
            await _uow.SaveChangesAsync();

            var result = new DoktorDTO
            {
                DoktorId = ent.DoktorId, // generira baza
                Ime = ent.Ime,
                Prezime = ent.Prezime
            };

            return Ok(ApiResponse<DoktorDTO>.Ok(result, "Doktor dodan."));
        }

        // PUT: /api/doktori/5
        // Body: { "ime": "...", "prezime": "..." }
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse<DoktorDTO>>> Update(int id, [FromBody] UpdateDoktorDTO dto)
        {
            var repo = _uow.Repository<Doktor>();
            var ent = await repo.GetByIdAsync(id);
            if (ent == null)
                return NotFound(ApiResponse<DoktorDTO>.Fail("Doktor nije pronađen."));

            if (string.IsNullOrWhiteSpace(dto.Ime) || string.IsNullOrWhiteSpace(dto.Prezime))
                return BadRequest(ApiResponse<DoktorDTO>.Fail("Ime i prezime su obavezni."));

            ent.Ime = dto.Ime.Trim();
            ent.Prezime = dto.Prezime.Trim();

            repo.Update(ent);
            await _uow.SaveChangesAsync();

            var result = new DoktorDTO
            {
                DoktorId = ent.DoktorId,
                Ime = ent.Ime,
                Prezime = ent.Prezime
            };

            return Ok(ApiResponse<DoktorDTO>.Ok(result, "Doktor ažuriran."));
        }

        // DELETE: /api/doktori/5
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
        {
            var repo = _uow.Repository<Doktor>();
            var ent = await repo.GetByIdAsync(id);
            if (ent == null)
                return NotFound(ApiResponse<object>.Fail("Doktor nije pronađen."));

            repo.Remove(ent);
            await _uow.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null, "Doktor obrisan."));
        }
    }
}
