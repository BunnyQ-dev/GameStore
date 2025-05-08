using Games_Store.Models;
using Games_Store.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Games_Store.DTOs;
using System.Security.Claims;

namespace Games_Store.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var userId = _userManager.GetUserId(User);
            var orders = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Game)
                .Where(o => o.UserId == userId)
                .ToListAsync();

            var orderDtos = orders.Select(o => new OrderDto
            {
                Id = o.Id,
                TotalAmount = o.TotalAmount,
                OrderDate = o.OrderDate,
                Status = o.Status,
                Items = o.Items.Select(i => new OrderItemDto
                {
                    GameId = i.GameId,
                    GameTitle = i.Game.Title,
                    Price = i.Price,
                    Quantity = i.Quantity
                }).ToList()
            });

            return Ok(orderDtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var userId = _userManager.GetUserId(User);
            var order = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Game)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);
            if (order == null) return NotFound();

            var orderDto = new OrderDto
            {
                Id = order.Id,
                TotalAmount = order.TotalAmount,
                OrderDate = order.OrderDate,
                Status = order.Status,
                Items = order.Items.Select(i => new OrderItemDto
                {
                    GameId = i.GameId,
                    GameTitle = i.Game.Title,
                    Price = i.Price,
                    Quantity = i.Quantity
                }).ToList()
            };

            return Ok(orderDto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] List<OrderItemCreateDto> items)
        {
            var userId = _userManager.GetUserId(User);
            if (items == null || !items.Any()) return BadRequest("No items to order.");

            var order = new Order
            {
                UserId = userId!,
                OrderDate = DateTime.UtcNow,
                Status = "Completed",
                Items = new List<OrderItem>(),
                TotalAmount = 0
            };

            foreach (var item in items)
            {
                var game = await _context.Games.FindAsync(item.GameId);
                if (game == null) return BadRequest($"Game with id {item.GameId} not found.");
                var orderItem = new OrderItem
                {
                    GameId = game.Id,
                    Price = game.Price,
                    Quantity = item.Quantity
                };
                order.Items.Add(orderItem);
                order.TotalAmount += game.Price * item.Quantity;
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var item in order.Items)
            {
                var alreadyPurchased = await _context.PurchasedGames
                    .AnyAsync(pg => pg.UserId == userId && pg.GameId == item.GameId);
                if (!alreadyPurchased)
                {
                    _context.PurchasedGames.Add(new PurchasedGame
                    {
                        UserId = userId!,
                        GameId = item.GameId,
                        PurchaseDate = DateTime.UtcNow
                    });
                }
            }
            await _context.SaveChangesAsync();

            var orderDto = new OrderDto
            {
                Id = order.Id,
                TotalAmount = order.TotalAmount,
                OrderDate = order.OrderDate,
                Status = order.Status,
                Items = order.Items.Select(i => new OrderItemDto
                {
                    GameId = i.GameId,
                    GameTitle = i.Game?.Title ?? _context.Games.FirstOrDefault(g => g.Id == i.GameId)?.Title ?? "Unknown Game",
                    Price = i.Price,
                    Quantity = i.Quantity
                }).ToList()
            };

            return Ok(orderDto);
        }

        // POST: api/order/checkout
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout()
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

            if (cart == null || (!cart.Items.Any() && !cart.CartBundles.Any()))
            {
                return BadRequest("Cart is empty.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                decimal totalAmount = 0;
                var orderItems = new List<OrderItem>();
                var orderBundles = new List<OrderBundle>();
                var purchasedGames = new List<PurchasedGame>();
                var gameIdsToRemoveFromWishlist = new List<int>();
                var processedGameIds = new HashSet<int>();

                foreach (var cartItem in cart.Items)
                {
                    var alreadyPurchased = await _context.PurchasedGames
                        .AnyAsync(pg => pg.GameId == cartItem.GameId && pg.UserId == userId);
                    if (alreadyPurchased || !processedGameIds.Add(cartItem.GameId)) continue;

                    decimal pricePaid = cartItem.Game.Price;
                    if (cartItem.Game.DiscountPercentage.HasValue && cartItem.Game.DiscountPercentage.Value > 0)
                    {
                        if (cartItem.Game.DiscountPercentage.Value < 100)
                        {
                            pricePaid = cartItem.Game.Price * (1 - cartItem.Game.DiscountPercentage.Value / 100m);
                        }
                        else
                        {
                            pricePaid = 0;
                        }
                    }
                    pricePaid = Math.Max(0, Math.Round(pricePaid, 2));
                    totalAmount += pricePaid * cartItem.Quantity;

                    orderItems.Add(new OrderItem
                    {
                        GameId = cartItem.GameId,
                        Quantity = cartItem.Quantity,
                        Price = pricePaid
                    });

                    purchasedGames.Add(new PurchasedGame
                    {
                        UserId = userId,
                        GameId = cartItem.GameId,
                        PurchaseDate = DateTime.UtcNow,
                        PricePaid = pricePaid
                    });
                    
                    gameIdsToRemoveFromWishlist.Add(cartItem.GameId);

                    cartItem.Game.Sales += cartItem.Quantity;
                    _context.Games.Update(cartItem.Game);
                }
                
                // Process bundles
                foreach (var cartBundle in cart.CartBundles)
                {
                    // Recalculate bundle price at checkout time to ensure accuracy
                    var bundleGames = cartBundle.Bundle.BundleGames.Select(bg => bg.Game).ToList();
                    var basePriceSum = bundleGames.Sum(g => 
                        Math.Round(
                            (g.DiscountPercentage.HasValue && g.DiscountPercentage.Value > 0)
                                ? (g.DiscountPercentage.Value < 100 ? g.Price * (1 - g.DiscountPercentage.Value / 100m) : 0)
                                : g.Price, 
                            2
                        )
                    );
                    var effectiveBundlePrice = cartBundle.Bundle.DiscountPrice ?? 
                                                (cartBundle.Bundle.DiscountPercentage.HasValue ? Math.Round(basePriceSum * (1 - cartBundle.Bundle.DiscountPercentage.Value / 100m), 2) :
                                                (cartBundle.Bundle.Price > 0 ? Math.Round(cartBundle.Bundle.Price, 2) : basePriceSum));

                    // Apply owned game discount
                    var ownedGameIdsInBundle = await _context.PurchasedGames
                        .Where(pg => pg.UserId == userId && cartBundle.Bundle.BundleGames.Select(bg => bg.GameId).Contains(pg.GameId))
                        .Select(pg => pg.GameId)
                        .ToListAsync();
                    decimal ownedDiscount = 0m;
                    if (ownedGameIdsInBundle.Any())
                    {
                        ownedDiscount = await _context.Games
                            .Where(g => ownedGameIdsInBundle.Contains(g.Id))
                            .SumAsync(g => 
                                Math.Round(
                                    (g.DiscountPercentage.HasValue && g.DiscountPercentage.Value > 0)
                                        ? (g.DiscountPercentage.Value < 100 ? g.Price * (1 - g.DiscountPercentage.Value / 100m) : 0)
                                        : g.Price, 
                                    2
                                )
                            );
                    }
                    var finalBundlePrice = Math.Max(0m, Math.Round(effectiveBundlePrice - ownedDiscount, 2));

                    // Add bundle to order
                    orderBundles.Add(new OrderBundle
                    {
                        BundleId = cartBundle.BundleId,
                        Quantity = cartBundle.Quantity,
                        PriceAtPurchase = finalBundlePrice
                    });
                    totalAmount += finalBundlePrice * cartBundle.Quantity;

                    // Add games from bundle to PurchasedGames (if not already owned or processed)
                    foreach (var gameInBundle in bundleGames)
                    {
                        if (!await _context.PurchasedGames.AnyAsync(pg => pg.UserId == userId && pg.GameId == gameInBundle.Id) && processedGameIds.Add(gameInBundle.Id))
                        {
                            purchasedGames.Add(new PurchasedGame
                            {
                                UserId = userId,
                                GameId = gameInBundle.Id,
                                PurchaseDate = DateTime.UtcNow,
                                PricePaid = 0
                            });
                            gameIdsToRemoveFromWishlist.Add(gameInBundle.Id);
                        }
                    }
                }

                if (!orderItems.Any() && !orderBundles.Any())
                {
                    await transaction.RollbackAsync();
                    return BadRequest("All items in the cart were already owned or could not be processed.");
                }

                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = totalAmount,
                    Status = "Completed",
                    Items = orderItems,
                    Bundles = orderBundles
                };
                _context.Orders.Add(order);

                await _context.PurchasedGames.AddRangeAsync(purchasedGames);

                var wishlistItemsToRemove = await _context.Wishlists
                    .Where(w => w.UserId == userId && gameIdsToRemoveFromWishlist.Contains(w.GameId))
                    .ToListAsync();
                _context.Wishlists.RemoveRange(wishlistItemsToRemove);

                _context.CartItems.RemoveRange(cart.Items);
                _context.CartBundles.RemoveRange(cart.CartBundles);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new { message = "Замовлення успішно оформлено.", orderId = order.Id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Checkout error: {ex}"); 
                return StatusCode(500, "Сталася помилка під час оформлення замовлення.");
            }
        }
        

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrderHistory()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var orders = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Game)
                .Where(o => o.UserId == userId)
                .ToListAsync();

            var orderDtos = orders.Select(o => new OrderDto
            {
                Id = o.Id,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                Items = o.Items.Select(oi => new OrderItemDto
                {
                    GameId = oi.GameId,
                    Title = oi.Game.Title,
                    Price = oi.Price,
                    Quantity = oi.Quantity,
                    ImageUrl = oi.Game.CoverImageUrl ?? ""
                }).ToList()
            }).ToList();

            return Ok(orderDtos);
        }
    }

    // DTOs
    public class OrderDto
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? UserEmail { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public List<OrderItemDto> Items { get; set; }
        public List<OrderBundleDto> Bundles { get; set; }
    }

    public class OrderItemDto
    {
        public int GameId { get; set; }
        public string? GameTitle { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Title { get; set; }
        public string ImageUrl { get; set; }
    }

    public class OrderItemCreateDto
    {
        public int GameId { get; set; }
        public int Quantity { get; set; }
    }
} 