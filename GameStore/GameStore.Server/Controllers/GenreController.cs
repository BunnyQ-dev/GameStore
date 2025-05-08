using Games_Store.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Games_Store.Data;

namespace Games_Store.Controllers
{
    [Route("api/genres")]
    [ApiController]
    public class GenreController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public GenreController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/genres
        [HttpGet]
        public async Task<IActionResult> GetGenres()
        {
            var genres = await _context.Genres.ToListAsync();
            return Ok(genres);
        }

        // GET: api/genres/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetGenre(int id)
        {
            var genre = await _context.Genres.FindAsync(id);
            if (genre == null) return NotFound();
            return Ok(genre);
        }

        // POST: api/genres
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateGenre([FromBody] Genre genre)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            _context.Genres.Add(genre);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetGenre), new { id = genre.Id }, genre);
        }

        // PUT: api/genres/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateGenre(int id, [FromBody] Genre genre)
        {
            if (id != genre.Id) return BadRequest();
            if (!ModelState.IsValid) return BadRequest(ModelState);
            _context.Entry(genre).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Genres.Any(g => g.Id == id))
                    return NotFound();
                throw;
            }
            return NoContent();
        }

        // DELETE: api/genres/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteGenre(int id)
        {
            var genre = await _context.Genres.FindAsync(id);
            if (genre == null) return NotFound();
            _context.Genres.Remove(genre);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
} 