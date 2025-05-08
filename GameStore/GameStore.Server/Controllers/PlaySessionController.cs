using Games_Store.Data;
using Games_Store.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Games_Store.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PlaySessionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PlaySessionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/PlaySession
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PlaySession>>> GetPlaySessions()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            return await _context.PlaySessions
                .Where(ps => ps.UserId == userId)
                .Include(ps => ps.Game)
                .ToListAsync();
        }

        // GET: api/PlaySession/Game/5
        [HttpGet("Game/{gameId}")]
        public async Task<ActionResult<IEnumerable<PlaySession>>> GetGamePlaySessions(int gameId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            return await _context.PlaySessions
                .Where(ps => ps.UserId == userId && ps.GameId == gameId)
                .OrderByDescending(ps => ps.StartTime)
                .ToListAsync();
        }

        // POST: api/PlaySession/Start
        [HttpPost("Start")]
        public async Task<ActionResult<PlaySession>> StartPlaySession(PlaySessionStartModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Check if game exists
            var gameExists = await _context.Games.AnyAsync(g => g.Id == model.GameId);
            if (!gameExists)
            {
                return NotFound("Game not found");
            }

            // Check if user owns the game
            var userOwnsGame = await _context.PurchasedGames
                .AnyAsync(pg => pg.UserId == userId && pg.GameId == model.GameId);

            if (!userOwnsGame)
            {
                return BadRequest("User does not own this game");
            }

            var playSession = new PlaySession
            {
                UserId = userId,
                GameId = model.GameId,
                StartTime = DateTime.UtcNow,
                DeviceInfo = model.DeviceInfo,
                HoursPlayed = 0 // Will be updated on end
            };

            _context.PlaySessions.Add(playSession);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPlaySession), new { id = playSession.Id }, playSession);
        }

        // PUT: api/PlaySession/End/5
        [HttpPut("End/{id}")]
        public async Task<IActionResult> EndPlaySession(int id, PlaySessionEndModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var playSession = await _context.PlaySessions
                .FirstOrDefaultAsync(ps => ps.Id == id && ps.UserId == userId);

            if (playSession == null)
            {
                return NotFound("Play session not found");
            }

            if (playSession.EndTime.HasValue)
            {
                return BadRequest("Play session already ended");
            }

            playSession.EndTime = DateTime.UtcNow;
            
            // Calculate hours played
            var duration = playSession.EndTime.Value - playSession.StartTime;
            playSession.HoursPlayed = (decimal)duration.TotalHours;

            _context.Entry(playSession).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/PlaySession/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PlaySession>> GetPlaySession(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var playSession = await _context.PlaySessions
                .Include(ps => ps.Game)
                .FirstOrDefaultAsync(ps => ps.Id == id && ps.UserId == userId);

            if (playSession == null)
            {
                return NotFound();
            }

            return playSession;
        }

        // GET: api/PlaySession/TotalHours/5
        [HttpGet("TotalHours/{gameId}")]
        public async Task<ActionResult<decimal>> GetTotalHoursPlayed(int gameId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            decimal totalHours = await _context.PlaySessions
                .Where(ps => ps.UserId == userId && ps.GameId == gameId)
                .SumAsync(ps => ps.HoursPlayed);

            return totalHours;
        }
    }

    public class PlaySessionStartModel
    {
        public int GameId { get; set; }
        public string? DeviceInfo { get; set; }
    }

    public class PlaySessionEndModel
    {
        public DateTime EndTime { get; set; } = DateTime.UtcNow;
    }
} 