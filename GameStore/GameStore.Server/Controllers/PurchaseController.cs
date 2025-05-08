using Games_Store.Data;
using Games_Store.Models;
using Games_Store.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Games_Store.Utils;

namespace Games_Store.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PurchaseController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PurchaseController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/purchase/status/{gameId}
        [HttpGet("status/{gameId}")]
        public async Task<ActionResult<bool>> GetPurchaseStatus(int gameId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var isPurchased = await _context.PurchasedGames
                .AnyAsync(pg => pg.GameId == gameId && pg.UserId == userId);
                
            return Ok(isPurchased);
        }

        // POST: api/purchase/{gameId}
        [HttpPost("{gameId}")]
        public async Task<IActionResult> PurchaseGame(int gameId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var game = await _context.Games.FindAsync(gameId);
            if (game == null) return NotFound("Game not found.");

            var alreadyPurchased = await _context.PurchasedGames
                .AnyAsync(pg => pg.GameId == gameId && pg.UserId == userId);

            if (alreadyPurchased)
            {
                return BadRequest("Game already purchased.");
            }

            decimal pricePaid = game.Price;
            if (game.DiscountPercentage.HasValue && game.DiscountPercentage.Value > 0)
            {
                if (game.DiscountPercentage.Value < 100)
                {
                    pricePaid = game.Price * (1 - game.DiscountPercentage.Value / 100m);
                }
                else
                {
                    pricePaid = 0;
                }
            }
            pricePaid = Math.Round(pricePaid, 2);
            
            var purchase = new PurchasedGame
            {
                UserId = userId,
                GameId = gameId,
                PurchaseDate = DateTime.UtcNow,
                PricePaid = pricePaid
            };

            _context.PurchasedGames.Add(purchase);

            
            await _context.SaveChangesAsync();

            return Ok(new { message = "Game purchased successfully." });
        }

        // GET: api/purchase/library
        [HttpGet("library")]
        public async Task<ActionResult<IEnumerable<GameDto>>> GetUserLibrary()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var purchasedGameEntities = await _context.PurchasedGames
                .Where(pg => pg.UserId == userId)
                .Include(pg => pg.Game)
                    .ThenInclude(g => g.GameGenres)
                        .ThenInclude(gg => gg.Genre)
                .Include(pg => pg.Game)
                    .ThenInclude(g => g.GameDevelopers)
                        .ThenInclude(gd => gd.Developer)
                 .Include(pg => pg.Game)
                    .ThenInclude(g => g.GamePublishers)
                        .ThenInclude(gp => gp.Publisher)
                 .Include(pg => pg.Game)
                    .ThenInclude(g => g.GamePlatforms)
                        .ThenInclude(gp => gp.Platform)
                 .Include(pg => pg.Game)
                    .ThenInclude(g => g.GameRatings)
                .Select(pg => pg.Game)
                .ToListAsync();
                
            var gameDtos = purchasedGameEntities
                .Select(g => DtoMapper.MapGameToDto(g))
                .ToList();

            foreach (var dto in gameDtos)
            {
                if (dto != null) dto.IsOwned = true; 
            }

            return Ok(gameDtos);
        }
    }
} 