using Games_Store.Data;
using Games_Store.DTOs;
using Games_Store.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Linq;

namespace Games_Store.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Кошик доступний тільки авторизованим користувачам
    public class CartController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/cart
        [HttpGet]
        public async Task<ActionResult<CartDto>> GetCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.Game)
                .Include(c => c.CartBundles)
                    .ThenInclude(cb => cb.Bundle)
                        .ThenInclude(b => b.BundleGames)
                            .ThenInclude(bg => bg.Game)
                .FirstOrDefaultAsync(c => c.UserId == userId);
            int finalCartId;
            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
                cart = await _context.Carts
                    .Include(c => c.Items).ThenInclude(ci => ci.Game)
                    .Include(c => c.CartBundles)
                        .ThenInclude(cb => cb.Bundle)
                            .ThenInclude(b => b.BundleGames)
                                .ThenInclude(bg => bg.Game)
                    .FirstAsync(c => c.UserId == userId);
            }
            finalCartId = cart.Id;

            // Map CartItems
            var itemsDto = await _context.CartItems
                .Where(ci => ci.CartId == finalCartId)
                .Select(ci => new CartItemDto
                {
                    Id = ci.Id,
                    GameId = ci.GameId,
                    Title = ci.Game.Title,
                    Price = Math.Round(
                        (ci.Game.DiscountPercentage.HasValue && ci.Game.DiscountPercentage.Value > 0) 
                            ? (ci.Game.DiscountPercentage.Value < 100 ? ci.Game.Price * (1 - ci.Game.DiscountPercentage.Value / 100m) : 0) 
                            : ci.Game.Price, 
                        2
                    ),
                    DiscountPercentage = ci.Game.DiscountPercentage,
                    OriginalPrice = (ci.Game.DiscountPercentage.HasValue && ci.Game.DiscountPercentage.Value > 0) ? (decimal?)Math.Round(ci.Game.Price, 2) : null,
                    ImageUrl = ci.Game.CoverImageUrl ?? string.Empty,
                    Quantity = ci.Quantity
                })
                .ToListAsync();

            // Map CartBundles with recalculated prices
            var bundlesDto = cart.CartBundles.Select(cb =>
            {
                var bundle = cb.Bundle;
                // Sum of game prices with their discounts
                var basePriceSum = bundle.BundleGames
                    .Select(bg => Math.Round(
                        (bg.Game.DiscountPercentage.HasValue && bg.Game.DiscountPercentage.Value > 0)
                            ? (bg.Game.DiscountPercentage.Value < 100 ? bg.Game.Price * (1 - bg.Game.DiscountPercentage.Value / 100m) : 0)
                            : bg.Game.Price, 
                        2
                    ))
                    .Sum();
                // Calculate effective price
                decimal effectiveBundlePrice = bundle.DiscountPrice.HasValue
                    ? bundle.DiscountPrice.Value
                    : (bundle.DiscountPercentage.HasValue && bundle.DiscountPercentage > 0
                        ? Math.Round(basePriceSum * (1 - bundle.DiscountPercentage.Value / 100m), 2)
                        : (bundle.Price > 0 ? Math.Round(bundle.Price, 2) : basePriceSum));
                return new CartBundleDto
                {
                    Id = cb.Id,
                    BundleId = cb.BundleId,
                    Name = bundle.Name,
                    Price = effectiveBundlePrice,
                    Quantity = cb.Quantity,
                    gameIds = bundle.BundleGames.Select(bg => bg.GameId).ToList()
                };
            }).ToList();

            var cartDto = new CartDto
            {
                Id = finalCartId,
                UserId = userId,
                Items = itemsDto,
                Bundles = bundlesDto
            };

            return Ok(cartDto);
        }

        // POST: api/cart/{gameId}
        [HttpPost("{gameId}")]
        public async Task<ActionResult<CartDto>> AddToCart(int gameId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var game = await _context.Games.FindAsync(gameId);
            if (game == null) return NotFound("Game not found.");

            var isPurchased = await _context.PurchasedGames.AnyAsync(pg => pg.GameId == gameId && pg.UserId == userId);
            if (isPurchased) return BadRequest("Ви вже придбали цю гру.");

            var cart = await GetOrCreateCartForUser(userId);
            var cartItem = cart.Items.FirstOrDefault(ci => ci.GameId == gameId);

            if (cartItem != null)
            {
                return BadRequest("Гра вже у кошику.");
            }
            else
            {
                cartItem = new CartItem
                {
                    CartId = cart.Id,
                    GameId = gameId,
                    Quantity = 1, // Завжди 1
                };
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();
            return await GetCart();
        }

        // DELETE: api/cart/{gameId}
        [HttpDelete("{gameId}")]
        public async Task<ActionResult<CartDto>> RemoveFromCart(int gameId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var cartItem = await _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.GameId == gameId && ci.Cart.UserId == userId);

            if (cartItem == null)
            {
                return NotFound("Гру не знайдено у кошику.");
            }

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            return await GetCart();
        }

        // DELETE: api/cart/clear
        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.Items.Any())
            {
                return NoContent();
            }

            _context.CartItems.RemoveRange(cart.Items);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        
        // POST: api/cart/bundles/{bundleId}
        [HttpPost("bundles/{bundleId}")]
        public async Task<ActionResult<CartDto>> AddBundleToCart(int bundleId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var bundle = await _context.Bundles
                .Include(b => b.BundleGames)
                    .ThenInclude(bg => bg.Game)
                .FirstOrDefaultAsync(b => b.Id == bundleId);
            if (bundle == null) return NotFound("Bundle not found.");

            // Compute base sum of game prices
            var basePriceSum = bundle.BundleGames
                .Select(bg => Math.Round(
                    (bg.Game.DiscountPercentage.HasValue && bg.Game.DiscountPercentage.Value > 0)
                        ? (bg.Game.DiscountPercentage.Value < 100 ? bg.Game.Price * (1 - bg.Game.DiscountPercentage.Value / 100m) : 0)
                        : bg.Game.Price, 
                    2
                ))
                .Sum();
            // Determine effective bundle price with overrides
            decimal effectiveBundlePrice = bundle.DiscountPrice.HasValue
                ? bundle.DiscountPrice.Value
                : (bundle.DiscountPercentage.HasValue && bundle.DiscountPercentage > 0
                    ? Math.Round(basePriceSum * (1 - bundle.DiscountPercentage.Value / 100m), 2)
                    : (bundle.Price > 0 ? Math.Round(bundle.Price, 2) : basePriceSum));

            // Determine games user already purchased for discount
            var bundleGameIds = bundle.BundleGames.Select(bg => bg.GameId).ToList();
            var ownedGameIds = await _context.PurchasedGames
                .Where(pg => pg.UserId == userId && bundleGameIds.Contains(pg.GameId))
                .Select(pg => pg.GameId)
                .ToListAsync();
            decimal ownedDiscount = 0m;
            if (ownedGameIds.Any())
            {
                ownedDiscount = await _context.Games
                    .Where(g => ownedGameIds.Contains(g.Id))
                    .SumAsync(g => 
                        (g.DiscountPercentage.HasValue && g.DiscountPercentage.Value > 0)
                            ? (g.DiscountPercentage.Value < 100 ? g.Price * (1 - g.DiscountPercentage.Value / 100m) : 0)
                            : g.Price
                    );
                ownedDiscount = Math.Round(ownedDiscount, 2);
            }
            // Final price subtracting owned games discount
            var finalBundlePrice = Math.Max(0m, Math.Round(effectiveBundlePrice - ownedDiscount, 2));

            // Add to cart
            var cart = await GetOrCreateCartForUser(userId);
            // Check if bundle already in cart
            if (cart.CartBundles.Any(cb => cb.BundleId == bundleId))
            {
                return BadRequest("Bundle already in cart.");
            }
            // Create CartBundle entry
            var cartBundle = new CartBundle
            {
                CartId = cart.Id,
                BundleId = bundleId,
                Quantity = 1,
                Price = finalBundlePrice
            };
            _context.CartBundles.Add(cartBundle);
            await _context.SaveChangesAsync();
            return await GetCart();
        }

        // DELETE: api/cart/bundles/{bundleId}
        [HttpDelete("bundles/{bundleId}")]
        public async Task<ActionResult<CartDto>> RemoveBundleFromCart(int bundleId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var cartBundle = await _context.CartBundles
                .Include(cb => cb.Cart)
                .FirstOrDefaultAsync(cb => cb.BundleId == bundleId && cb.Cart.UserId == userId);
            if (cartBundle == null) return NotFound("Bundle not found in cart.");

            _context.CartBundles.Remove(cartBundle);
            await _context.SaveChangesAsync();
            return await GetCart();
        }

        private async Task<Cart> GetOrCreateCartForUser(string userId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.Game)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
                 cart = await _context.Carts
                    .Include(c => c.Items)
                        .ThenInclude(ci => ci.Game)
                    .FirstAsync(c => c.UserId == userId); 
            }
            return cart;
        }
    }
} 