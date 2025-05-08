using Games_Store.Models;
using Games_Store.DTOs;
using Games_Store.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Games_Store.Controllers
{
    [Route("api/publishers")]
    [ApiController]
    public class PublisherController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public PublisherController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/publishers
        [HttpGet]
        public async Task<IActionResult> GetPublishers()
        {
            var publishers = await _context.Publishers.ToListAsync();
            var dtos = publishers.Select(p => new PublisherDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Website = p.Website,
                LogoUrl = p.LogoUrl,
                FoundedDate = p.FoundedDate
            }).ToList();
            return Ok(dtos);
        }

        // GET: api/publishers/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPublisher(int id)
        {
            var p = await _context.Publishers.FindAsync(id);
            if (p == null) return NotFound();
            var dto = new PublisherDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Website = p.Website,
                LogoUrl = p.LogoUrl,
                FoundedDate = p.FoundedDate
            };
            return Ok(dto);
        }

        // POST: api/publishers
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreatePublisher([FromBody] PublisherDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var pub = new Publisher
            {
                Name = dto.Name,
                Description = dto.Description,
                Website = dto.Website,
                LogoUrl = dto.LogoUrl,
                FoundedDate = dto.FoundedDate
            };
            _context.Publishers.Add(pub);
            await _context.SaveChangesAsync();
            dto.Id = pub.Id;
            return CreatedAtAction(nameof(GetPublisher), new { id = pub.Id }, dto);
        }

        // PUT: api/publishers/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdatePublisher(int id, [FromBody] PublisherDto dto)
        {
            if (id != dto.Id) return BadRequest();
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var pub = await _context.Publishers.FindAsync(id);
            if (pub == null) return NotFound();
            pub.Name = dto.Name;
            pub.Description = dto.Description;
            pub.Website = dto.Website;
            pub.LogoUrl = dto.LogoUrl;
            pub.FoundedDate = dto.FoundedDate;
            _context.Entry(pub).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/publishers/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePublisher(int id)
        {
            var pub = await _context.Publishers.FindAsync(id);
            if (pub == null) return NotFound();
            _context.Publishers.Remove(pub);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
} 