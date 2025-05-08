using Games_Store.Data;
using Games_Store.Models;
using Games_Store.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Linq;

namespace Games_Store.Controllers
{
    [Route("api/library")]
    [ApiController]
    [Authorize]
    public class LibraryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public LibraryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/library
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GameDto>>> GetLibrary()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var purchasedGames = await _context.PurchasedGames
                .Where(pg => pg.UserId == userId)
                .Include(pg => pg.Game)
                    .ThenInclude(g => g.GameGenres).ThenInclude(gg => gg.Genre)
                .Include(pg => pg.Game)
                    .ThenInclude(g => g.GamePlatforms).ThenInclude(gp => gp.Platform)
                .Include(pg => pg.Game)
                    .ThenInclude(g => g.GameDevelopers).ThenInclude(gd => gd.Developer)
                .Include(pg => pg.Game)
                    .ThenInclude(g => g.GamePublishers).ThenInclude(gp => gp.Publisher)
                .Include(pg => pg.Game)
                    .ThenInclude(g => g.GameRatings)
                .ToListAsync();

            var games = purchasedGames.Select(pg => pg.Game).Distinct();

            var gameDtos = games.Select(game => new GameDto
            {
                Id = game.Id,
                Title = game.Title,
                Description = game.Description,
                ReleaseDate = game.ReleaseDate,
                Price = game.Price,
                CoverImageUrl = game.CoverImageUrl,
                TrailerUrl = game.TrailerUrl,
                IsActive = game.IsActive,
                Genres = game.GameGenres.Select(gg => gg.Genre.Name).ToList(),
                Platforms = game.GamePlatforms.Select(gp => gp.Platform.Name).ToList(),
                Developers = game.GameDevelopers.Select(gd => gd.Developer.Name).ToList(),
                DeveloperIds = game.GameDevelopers.Select(gd => gd.DeveloperId).ToList(),
                Publishers = game.GamePublishers.Select(gp => gp.Publisher.Name).ToList(),
                PublisherIds = game.GamePublishers.Select(gp => gp.PublisherId).ToList(),
                Rating = game.GameRatings.Any() ? Math.Round(game.GameRatings.Average(r => r.Score), 2) : 0
            }).ToList();

            return gameDtos;
        }

        // GET: api/library/owns/{gameId}
        [HttpGet("owns/{gameId}")]
        public async Task<ActionResult<bool>> OwnsGame(int gameId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            var owns = await _context.PurchasedGames.AnyAsync(pg => pg.UserId == userId && pg.GameId == gameId);
            return owns;
        }
    }
} 