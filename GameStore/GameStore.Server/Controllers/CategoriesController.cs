using Games_Store.Data;
using Games_Store.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Games_Store.Controllers
{
    [Route("api/categories")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetCategories()
        {
            var genres = await _context.Genres
                .Select(g => new
                {
                    id = g.Id,
                    name = g.Name,
                    slug = g.Name.ToLower().Replace(" ", "-"),
                    type = "genre"
                })
                .ToListAsync();

            return Ok(genres);
        }

        // GET: api/categories/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetCategory(int id)
        {
            var genre = await _context.Genres.FindAsync(id);

            if (genre == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                id = genre.Id,
                name = genre.Name,
                slug = genre.Name.ToLower().Replace(" ", "-"),
                type = "genre"
            });
        }

        // POST: api/categories
        [HttpPost]
        public async Task<ActionResult<object>> CreateCategory(CategoryCreateDto category)
        {
            if (await _context.Genres.AnyAsync(g => g.Name == category.Name))
            {
                return Conflict("Category with this name already exists");
            }

            var genre = new Genre
            {
                Name = category.Name,
                Description = category.Description
            };

            _context.Genres.Add(genre);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategory), new { id = genre.Id }, new
            {
                id = genre.Id,
                name = genre.Name,
                slug = genre.Name.ToLower().Replace(" ", "-"),
                type = "genre"
            });
        }

        // PUT: api/categories/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, CategoryUpdateDto category)
        {
            var genre = await _context.Genres.FindAsync(id);
            if (genre == null)
            {
                return NotFound();
            }

            if (category.Name != genre.Name && await _context.Genres.AnyAsync(g => g.Name == category.Name))
            {
                return Conflict("Category with this name already exists");
            }

            genre.Name = category.Name;
            genre.Description = category.Description;

            _context.Entry(genre).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/categories/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var genre = await _context.Genres.FindAsync(id);
            if (genre == null)
            {
                return NotFound();
            }

            var hasGames = await _context.GameGenres.AnyAsync(gg => gg.GenreId == id);
            if (hasGames)
            {
                return BadRequest("Cannot delete category because it contains games");
            }

            _context.Genres.Remove(genre);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CategoryExists(int id)
        {
            return _context.Genres.Any(e => e.Id == id);
        }
    }

    public class CategoryCreateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class CategoryUpdateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
} 