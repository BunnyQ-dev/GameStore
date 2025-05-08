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
    public class WishlistController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public WishlistController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/wishlist/status/{gameId}
        [HttpGet("status/{gameId}")]
        public async Task<ActionResult<bool>> GetWishlistStatus(int gameId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }
            
            var isInWishlist = await _context.Wishlists
                .AnyAsync(w => w.GameId == gameId && w.UserId == userId);
                
            return Ok(isInWishlist);
        }

        // POST: api/wishlist/{gameId}
        [HttpPost("{gameId}")]
        public async Task<IActionResult> AddToWishlist(int gameId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var gameExists = await _context.Games.AnyAsync(g => g.Id == gameId);
            if (!gameExists) return NotFound("Game not found.");

            var alreadyInWishlist = await _context.Wishlists
                .AnyAsync(w => w.GameId == gameId && w.UserId == userId);

            if (alreadyInWishlist)
            {
                return BadRequest("Game already in wishlist.");
            }

            var wishlistItem = new Wishlist
            {
                UserId = userId,
                GameId = gameId,
                AddedDate = DateTime.UtcNow
            };

            _context.Wishlists.Add(wishlistItem);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Game added to wishlist." });
        }

        // DELETE: api/wishlist/{gameId}
        [HttpDelete("{gameId}")]
        public async Task<IActionResult> RemoveFromWishlist(int gameId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var wishlistItem = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.GameId == gameId && w.UserId == userId);

            if (wishlistItem == null)
            {
                return NotFound("Game not found in wishlist.");
            }

            _context.Wishlists.Remove(wishlistItem);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Game removed from wishlist." });
        }
        
        // GET: api/wishlist
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GameDto>>> GetWishlist()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            // 1. Get IDs of games owned by the user
            HashSet<int>? ownedGameIds = await _context.PurchasedGames
                .Where(pg => pg.UserId == userId)
                .Select(pg => pg.GameId)
                .ToHashSetAsync();

            // 2. Fetch wishlist game entities including related data
            var wishlistGameEntities = await _context.Wishlists
                .Where(w => w.UserId == userId)
                .Include(w => w.Game)
                    .ThenInclude(g => g.GameGenres)
                        .ThenInclude(gg => gg.Genre)
                .Include(w => w.Game)
                    .ThenInclude(g => g.GameDevelopers)
                        .ThenInclude(gd => gd.Developer)
                 .Include(w => w.Game)
                    .ThenInclude(g => g.GamePublishers)
                        .ThenInclude(gp => gp.Publisher)
                 .Include(w => w.Game)
                    .ThenInclude(g => g.GamePlatforms)
                        .ThenInclude(gp => gp.Platform)
                 .Include(w => w.Game)
                    .ThenInclude(g => g.GameRatings)
                .Select(w => w.Game)
                .ToListAsync();

            // 3. Map entities to DTOs in memory, passing ownedGameIds
            var gameDtos = wishlistGameEntities
                .Select(g => DtoMapper.MapGameToDto(g, ownedGameIds)) 
                .ToList();

            return Ok(gameDtos);
        }
    }
} 