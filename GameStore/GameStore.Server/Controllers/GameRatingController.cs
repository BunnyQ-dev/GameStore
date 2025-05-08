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
    public class GameRatingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GameRatingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/GameRating/Game/5
        [HttpGet("Game/{gameId}")]
        public async Task<ActionResult<IEnumerable<GameRatingViewModel>>> GetGameRatings(int gameId)
        {
            var ratings = await _context.GameRatings
                .Where(gr => gr.GameId == gameId)
                .Include(gr => gr.User)
                .Select(gr => new GameRatingViewModel
                {
                    UserId = gr.UserId,
                    UserName = gr.User.UserName,
                    DisplayName = gr.User.DisplayName,
                    GameId = gr.GameId,
                    Score = gr.Score,
                    Comment = gr.Comment,
                    RatingDate = gr.RatingDate,
                    LastUpdated = gr.LastUpdated
                })
                .ToListAsync();

            return ratings;
        }

        // GET: api/GameRating/Game/5/User
        [HttpGet("Game/{gameId}/User")]
        [Authorize]
        public async Task<ActionResult<GameRatingViewModel>> GetUserGameRating(int gameId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var rating = await _context.GameRatings
                .Where(gr => gr.GameId == gameId && gr.UserId == userId)
                .Include(gr => gr.User)
                .Select(gr => new GameRatingViewModel
                {
                    UserId = gr.UserId,
                    UserName = gr.User.UserName,
                    DisplayName = gr.User.DisplayName,
                    GameId = gr.GameId,
                    Score = gr.Score,
                    Comment = gr.Comment,
                    RatingDate = gr.RatingDate,
                    LastUpdated = gr.LastUpdated
                })
                .FirstOrDefaultAsync();

            if (rating == null)
            {
                return NotFound("Rating not found");
            }

            return rating;
        }

        // POST: api/GameRating
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<GameRating>> CreateGameRating(GameRatingCreateModel model)
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

            // Check if user already rated this game
            var existingRating = await _context.GameRatings
                .FirstOrDefaultAsync(gr => gr.UserId == userId && gr.GameId == model.GameId);

            if (existingRating != null)
            {
                return Conflict("User has already rated this game. Use PUT to update rating.");
            }

            // Create new rating
            var gameRating = new GameRating
            {
                UserId = userId,
                GameId = model.GameId,
                Score = model.Score,
                Comment = model.Comment,
                RatingDate = DateTime.UtcNow
            };

            _context.GameRatings.Add(gameRating);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUserGameRating), new { gameId = model.GameId }, gameRating);
        }

        // PUT: api/GameRating/5
        [HttpPut("{gameId}")]
        [Authorize]
        public async Task<IActionResult> UpdateGameRating(int gameId, GameRatingUpdateModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var gameRating = await _context.GameRatings
                .FirstOrDefaultAsync(gr => gr.UserId == userId && gr.GameId == gameId);

            if (gameRating == null)
            {
                return NotFound("Rating not found");
            }

            gameRating.Score = model.Score;
            gameRating.Comment = model.Comment;
            gameRating.LastUpdated = DateTime.UtcNow;

            _context.Entry(gameRating).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/GameRating/5
        [HttpDelete("{gameId}")]
        [Authorize]
        public async Task<IActionResult> DeleteGameRating(int gameId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var gameRating = await _context.GameRatings
                .FirstOrDefaultAsync(gr => gr.UserId == userId && gr.GameId == gameId);

            if (gameRating == null)
            {
                return NotFound("Rating not found");
            }

            _context.GameRatings.Remove(gameRating);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/GameRating/AverageScore/5
        [HttpGet("AverageScore/{gameId}")]
        public async Task<ActionResult<double>> GetAverageScore(int gameId)
        {
            if (!await _context.Games.AnyAsync(g => g.Id == gameId))
            {
                return NotFound("Game not found");
            }

            var ratings = await _context.GameRatings
                .Where(gr => gr.GameId == gameId)
                .ToListAsync();

            if (ratings.Count == 0)
            {
                return 0;
            }

            return ratings.Average(r => r.Score);
        }
    }

    public class GameRatingViewModel
    {
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string? DisplayName { get; set; }
        public int GameId { get; set; }
        public int Score { get; set; }
        public string? Comment { get; set; }
        public DateTime RatingDate { get; set; }
        public DateTime? LastUpdated { get; set; }
    }

    public class GameRatingCreateModel
    {
        public int GameId { get; set; }
        public int Score { get; set; }
        public string? Comment { get; set; }
    }

    public class GameRatingUpdateModel
    {
        public int Score { get; set; }
        public string? Comment { get; set; }
    }
} 