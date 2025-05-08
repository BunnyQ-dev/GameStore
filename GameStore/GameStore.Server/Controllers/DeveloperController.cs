using Games_Store.Models;
using Games_Store.DTOs;
using Games_Store.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Games_Store.Controllers
{
    [Route("api/developers")]
    [ApiController]
    public class DeveloperController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public DeveloperController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/developers
        [HttpGet]
        public async Task<IActionResult> GetDevelopers()
        {
            var developers = await _context.Developers.ToListAsync();
            var dtos = developers.Select(d => new DeveloperDto
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                Website = d.Website,
                LogoUrl = d.LogoUrl,
                FoundedDate = d.FoundedDate
            }).ToList();
            return Ok(dtos);
        }

        // GET: api/developers/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDeveloper(int id)
        {
            var d = await _context.Developers.FindAsync(id);
            if (d == null) return NotFound();
            var dto = new DeveloperDto
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                Website = d.Website,
                LogoUrl = d.LogoUrl,
                FoundedDate = d.FoundedDate
            };
            return Ok(dto);
        }

        // POST: api/developers
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateDeveloper([FromBody] DeveloperDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var dev = new Developer
            {
                Name = dto.Name,
                Description = dto.Description,
                Website = dto.Website,
                LogoUrl = dto.LogoUrl,
                FoundedDate = dto.FoundedDate
            };
            _context.Developers.Add(dev);
            await _context.SaveChangesAsync();
            dto.Id = dev.Id;
            return CreatedAtAction(nameof(GetDeveloper), new { id = dev.Id }, dto);
        }

        // PUT: api/developers/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateDeveloper(int id, [FromBody] DeveloperDto dto)
        {
            if (id != dto.Id) return BadRequest();
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var dev = await _context.Developers.FindAsync(id);
            if (dev == null) return NotFound();
            dev.Name = dto.Name;
            dev.Description = dto.Description;
            dev.Website = dto.Website;
            dev.LogoUrl = dto.LogoUrl;
            dev.FoundedDate = dto.FoundedDate;
            _context.Entry(dev).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/developers/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteDeveloper(int id)
        {
            var dev = await _context.Developers.FindAsync(id);
            if (dev == null) return NotFound();
            _context.Developers.Remove(dev);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
} 