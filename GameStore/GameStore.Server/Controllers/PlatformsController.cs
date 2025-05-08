using Games_Store.Data;
using Games_Store.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Games_Store.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace Games_Store.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlatformsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PlatformsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/platforms
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Platform>>> GetPlatforms()
        {
            var platforms = await _context.Platforms.OrderBy(p => p.Name).ToListAsync();
            return Ok(platforms);
        }

        // GET: api/Platforms/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Platform>> GetPlatform(int id)
        {
            var platform = await _context.Platforms.FindAsync(id);

            if (platform == null)
            {
                return NotFound();
            }

            return platform;
        }
        
        // POST: api/Platforms
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Platform>> PostPlatform(CreatePlatformDto platformDto)
        {
            var existingPlatform = await _context.Platforms
                .FirstOrDefaultAsync(p => p.Name.ToLower() == platformDto.Name.ToLower());

            if (existingPlatform != null)
            {
                return Conflict(new { message = $"Platform with name '{platformDto.Name}' already exists." });
            }

            var platform = new Platform
            {
                Name = platformDto.Name
            };

            _context.Platforms.Add(platform);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPlatform), new { id = platform.Id }, platform);
        }

        
    }
} 