using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Games_Store.Data;
using Games_Store.Models;
using Games_Store.DTOs;
using System.Security.Claims;

namespace Games_Store.Controllers
{
    [Route("api/bundles")]
    [ApiController]
    public class BundlesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BundlesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BundleDto>>> GetBundles()
        {
            var bundles = await _context.Bundles
                .Include(b => b.BundleGames)
                .ToListAsync();

            var dtos = bundles.Select(b => new BundleDto
            {
                Id = b.Id,
                Name = b.Name,
                Description = b.Description,
                Price = b.Price,
                DiscountPercentage = b.DiscountPercentage,
                DiscountPrice = b.DiscountPrice,
                StartDate = b.StartDate,
                EndDate = b.EndDate,
                ImageUrl = b.ImageUrl,
                IsActive = b.IsActive,
                GameIds = b.BundleGames.Select(bg => bg.GameId).ToList()
            }).ToList();

            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BundleDto>> GetBundle(int id)
        {
            var bundle = await _context.Bundles
                .Include(b => b.BundleGames)
                    .ThenInclude(bg => bg.Game)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bundle == null)
            {
                return NotFound();
            }

            // Determine which games in the bundle the current user already owns
            var ownedGameIds = new List<int>();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                ownedGameIds = await _context.PurchasedGames
                    .Where(pg => pg.UserId == userId && bundle.BundleGames.Select(bg => bg.GameId).Contains(pg.GameId))
                    .Select(pg => pg.GameId)
                    .ToListAsync();
            }
            // Calculate total discount (sum of prices of owned games)
            decimal ownedDiscount = 0m;
            if (ownedGameIds.Any())
            {
                ownedDiscount = await _context.Games
                    .Where(g => ownedGameIds.Contains(g.Id))
                    .SumAsync(g => g.Price);
            }
            var finalPrice = bundle.Price - ownedDiscount;

            var dto = new BundleDto
            {
                Id = bundle.Id,
                Name = bundle.Name,
                Description = bundle.Description,
                Price = bundle.Price,
                DiscountPercentage = bundle.DiscountPercentage,
                DiscountPrice = bundle.DiscountPrice,
                StartDate = bundle.StartDate,
                EndDate = bundle.EndDate,
                ImageUrl = bundle.ImageUrl,
                IsActive = bundle.IsActive,
                GameIds = bundle.BundleGames.Select(bg => bg.GameId).ToList(),
                Games = bundle.BundleGames.Select(bg => new BundleGameDto
                {
                    GameId = bg.GameId,
                    Title = bg.Game.Title,
                    ImageUrl = bg.Game.CoverImageUrl ?? string.Empty
                }).ToList(),
                OwnedGameIds = ownedGameIds,
                OwnedDiscount = Math.Round(ownedDiscount, 2),
                FinalPrice = Math.Max(0, Math.Round(finalPrice, 2))
            };

            return Ok(dto);
        }
    }
} 