using Microsoft.AspNetCore.Mvc;
using MedicalSystemApp.Data.Repositories;
using MedicalSystemApp.Models;
using MedicalSystemApp.Models.DTO;

namespace MedicalSystemApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DokumentiController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        private readonly IWebHostEnvironment _env;

        public DokumentiController(IUnitOfWork uow, IWebHostEnvironment env)
        {
            _uow = uow;
            _env = env;
        }

        // POST /api/dokumenti/upload?pregledId=1
        // Body: multipart/form-data (key: file)
        [HttpPost("upload")]
        public async Task<ActionResult<ApiResponse<UploadResultDTO>>> Upload([FromQuery] int pregledId, IFormFile file)
        {
            // Provjeri FK: pregled
            var pregled = await _uow.Repository<Pregled>().GetByIdAsync(pregledId);
            if (pregled == null)
                return BadRequest(ApiResponse<UploadResultDTO>.Fail("Pregled s navedenim ID-om ne postoji."));

            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse<UploadResultDTO>.Fail("Datoteka nije poslana."));

            // Dozvoljene ekstenzije (po potrebi proširi)
            var allowed = new[] { ".png", ".jpg", ".jpeg", ".pdf" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowed.Contains(ext))
                return BadRequest(ApiResponse<UploadResultDTO>.Fail("Nedozvoljena ekstenzija (dozvoljeno: .png, .jpg, .jpeg, .pdf)."));

            // Spremi u wwwroot/uploads
            var webroot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploads = Path.Combine(webroot, "uploads");
            Directory.CreateDirectory(uploads);

            var safeName = Path.GetFileName(file.FileName);
            var storedName = $"{Guid.NewGuid():N}_{safeName}";
            var fullPath = Path.Combine(uploads, storedName);

            await using (var fs = new FileStream(fullPath, FileMode.Create))
                await file.CopyToAsync(fs);

            var relativeUrl = $"/uploads/{storedName}";

            var ent = new Dokument
            {
                PregledId = pregledId,
                NazivDatoteke = safeName,
                Putanja = relativeUrl,
                TipDokumenta = file.ContentType,
                DatumUnosa = DateTime.UtcNow
            };

            try
            {
                await _uow.Repository<Dokument>().AddAsync(ent);
                await _uow.SaveChangesAsync();

                return Ok(ApiResponse<UploadResultDTO>.Ok(new UploadResultDTO
                {
                    DokumentId = ent.DokumentId,
                    Poruka = "Dokument uploadan.",
                    Url = relativeUrl
                }, "Dokument uspješno spremljen."));
            }
            catch
            {
                // Best-effort čišćenje datoteke ako insert padne
                try { if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath); } catch {}
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<UploadResultDTO>.Fail("Greška pri spremanju dokumenta."));
            }
        }

        // GET /api/dokumenti/pregled/1  -> lista dokumenata za pregled
        [HttpGet("pregled/{pregledId:int}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<DokumentDTO>>>> ForPregled(int pregledId)
        {
            var list = await _uow.Repository<Dokument>().FindAsync(d => d.PregledId == pregledId);
            var dto = list.Select(d => new DokumentDTO
            {
                DokumentId = d.DokumentId,
                PregledId = d.PregledId,
                NazivDatoteke = d.NazivDatoteke,
                Putanja = d.Putanja,
                TipDokumenta = d.TipDokumenta,
                DatumUnosa = d.DatumUnosa.ToLocalTime().ToString("dd.MM.yyyy. HH:mm")
            });
            return Ok(ApiResponse<IEnumerable<DokumentDTO>>.Ok(dto, "Dokumenti dohvaćeni."));
        }

        // GET /api/dokumenti/download/10  -> preuzmi datoteku
        [HttpGet("download/{id:int}")]
        public async Task<IActionResult> Download(int id)
        {
            var ent = await _uow.Repository<Dokument>().GetByIdAsync(id);
            if (ent == null) return NotFound(ApiResponse<object>.Fail("Dokument nije pronađen."));

            var webroot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var fullPath = Path.Combine(webroot, ent.Putanja.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            if (!System.IO.File.Exists(fullPath))
                return NotFound(ApiResponse<object>.Fail("Fizička datoteka nije pronađena."));

            var bytes = await System.IO.File.ReadAllBytesAsync(fullPath);
            var contentType = string.IsNullOrWhiteSpace(ent.TipDokumenta) ? "application/octet-stream" : ent.TipDokumenta;
            return File(bytes, contentType, ent.NazivDatoteke);
        }

        // DELETE /api/dokumenti/10
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
        {
            var repo = _uow.Repository<Dokument>();
            var ent = await repo.GetByIdAsync(id);
            if (ent == null) return NotFound(ApiResponse<object>.Fail("Dokument nije pronađen."));

            // Pokušaj obrisati i datoteku (best-effort)
            try
            {
                var webroot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var fullPath = Path.Combine(webroot, ent.Putanja.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);
            }
            catch { /* ignore */ }

            try
            {
                repo.Remove(ent);
                await _uow.SaveChangesAsync();
                return Ok(ApiResponse<object>.Ok(null, "Dokument izbrisan."));
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail("Greška pri brisanju dokumenta."));
            }
        }
    }
}
