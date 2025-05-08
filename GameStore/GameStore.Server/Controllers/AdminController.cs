using Microsoft.AspNetCore.Mvc;
using Games_Store.Attributes;
using Games_Store.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Games_Store.Data;
using Games_Store.DTOs;

namespace Games_Store.Controllers
{
    [Route("api/admin")]
    [ApiController]
    [RequireRole("Admin")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var totalGames = await _context.Games.CountAsync();
            var totalUsers = await _userManager.Users.CountAsync();
            var totalOrders = await _context.Orders.CountAsync();
            var totalRevenue = await _context.Orders
                .Where(o => o.Status == "Completed")
                .SumAsync(o => o.TotalAmount);

            return Ok(new
            {
                totalGames,
                totalUsers,
                totalOrders,
                totalRevenue
            });
        }

        [HttpGet("recent-orders")]
        public async Task<IActionResult> GetRecentOrders()
        {
            var recentOrders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .Select(o => new
                {
                    o.Id,
                    userName = o.User.UserName,
                    o.OrderDate,
                    total = o.TotalAmount,
                    o.Status,
                    itemCount = o.Items.Count
                })
                .ToListAsync();

            return Ok(recentOrders);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var usersList = new List<object>();
            
            var users = await _userManager.Users.ToListAsync();
            
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                
                usersList.Add(new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.DisplayName,
                    user.RegistrationDate,
                    Roles = roles
                });
            }

            return Ok(usersList);
        }

        [HttpGet("games")]
        public async Task<IActionResult> GetGames()
        {
            var games = await _context.Games
                .Include(g => g.GameDevelopers).ThenInclude(gd => gd.Developer)
                .Include(g => g.GamePublishers).ThenInclude(gp => gp.Publisher)
                .Include(g => g.GameGenres).ThenInclude(gg => gg.Genre)
                .Select(g => new
                {
                    g.Id,
                    g.Title,
                    g.Price,
                    g.DiscountPrice,
                    g.DiscountPercentage,
                    g.ReleaseDate,
                    g.IsActive,
                    Developers = g.GameDevelopers.Select(gd => gd.Developer.Name).ToList(),
                    Publishers = g.GamePublishers.Select(gp => gp.Publisher.Name).ToList(),
                    Genres = g.GameGenres.Select(gg => gg.Genre.Name).ToList()
                })
                .ToListAsync();

            return Ok(games);
        }

        [HttpGet("orders")]
        public async Task<IActionResult> GetOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Game)
                .Include(o => o.Bundles)
                    .ThenInclude(ob => ob.Bundle)
                .OrderByDescending(o => o.OrderDate);

            var totalOrders = await query.CountAsync();
            var orders = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var orderDtos = orders.Select(o => new OrderDto
            {
                Id = o.Id,
                UserId = o.UserId,
                UserEmail = o.User.Email,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                Items = o.Items.Select(i => new OrderItemDto
                {
                    GameId = i.GameId,
                    GameTitle = i.Game.Title,
                    Price = i.Price,
                    Quantity = i.Quantity
                }).ToList(),
                Bundles = o.Bundles.Select(ob => new OrderBundleDto
                {
                    BundleId = ob.BundleId,
                    BundleName = ob.Bundle.Name,
                    Quantity = ob.Quantity,
                    PriceAtPurchase = ob.PriceAtPurchase
                }).ToList()
            }).ToList();

            return Ok(new { orders = orderDtos, totalOrders, pages = (int)Math.Ceiling((double)totalOrders / pageSize) });
        }

        [HttpGet("orders/{id}")]
        public async Task<IActionResult> GetOrderDetails(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Game)
                .Include(o => o.Bundles)
                    .ThenInclude(ob => ob.Bundle)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            var orderDto = new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                UserEmail = order.User.Email,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                Items = order.Items.Select(i => new OrderItemDto
                {
                    GameId = i.GameId,
                    GameTitle = i.Game.Title,
                    Price = i.Price,
                    Quantity = i.Quantity
                }).ToList(),
                Bundles = order.Bundles.Select(ob => new OrderBundleDto
                {
                    BundleId = ob.BundleId,
                    BundleName = ob.Bundle.Name,
                    Quantity = ob.Quantity,
                    PriceAtPurchase = ob.PriceAtPurchase
                }).ToList()
            };

            return Ok(orderDto);
        }

        [HttpPut("orders/{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusModel model)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound("Order not found");

            order.Status = model.Status;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Order status updated successfully" });
        }

        [HttpPost("games")]
        public async Task<IActionResult> AddGame([FromBody] AddGameModel model)
        {
            var developer = await _context.Developers.FindAsync(model.DeveloperId);
            var publisher = await _context.Publishers.FindAsync(model.PublisherId);

            if (developer == null || publisher == null)
                return BadRequest("Developer or Publisher not found");

            var game = new Game
            {
                Title = model.Title,
                Description = model.Description,
                Price = model.Price,
                ReleaseDate = model.ReleaseDate,
                CoverImageUrl = model.CoverImageUrl,
                BackgroundImageUrl = model.BackgroundImageUrl,
                TrailerUrl = model.TrailerUrl,
                MinimumSystemRequirements = model.MinimumSystemRequirements,
                RecommendedSystemRequirements = model.RecommendedSystemRequirements,
                DiscountPercentage = model.DiscountPercentage,
                IsFeatured = model.IsFeatured,
                IsActive = true
            };

            _context.Games.Add(game);
            await _context.SaveChangesAsync();

            _context.GameDevelopers.Add(new GameDeveloper { GameId = game.Id, DeveloperId = model.DeveloperId });
            _context.GamePublishers.Add(new GamePublisher { GameId = game.Id, PublisherId = model.PublisherId });

            foreach (var genreId in model.GenreIds)
            {
                _context.GameGenres.Add(new GameGenre { GameId = game.Id, GenreId = genreId });
            }

            foreach (var platformId in model.PlatformIds)
            {
                _context.GamePlatforms.Add(new GamePlatform { GameId = game.Id, PlatformId = platformId });
            }

            if (model.Screenshots != null)
            {
                foreach (var url in model.Screenshots)
                {
                    _context.Screenshots.Add(new Screenshot { GameId = game.Id, Url = url });
                }
            }

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetGameDetails), new { id = game.Id }, new
            {
                game.Id,
                game.Title,
                message = "Game added successfully"
            });
        }

        [HttpPut("games/{id}")]
        public async Task<IActionResult> UpdateGame(int id, [FromBody] UpdateGameModel model)
        {
            var game = await _context.Games
                .Include(g => g.Genres)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (game == null)
                return NotFound("Game not found");

            game.Title = model.Title ?? game.Title;
            game.Description = model.Description ?? game.Description;

            if (model.Price.HasValue)
                game.Price = model.Price.Value;
            
            if (model.ReleaseDate.HasValue)
                game.ReleaseDate = model.ReleaseDate.Value;
            
            game.ImageUrl = model.ImageUrl ?? game.ImageUrl;
            game.VideoUrl = model.VideoUrl ?? game.VideoUrl;
            game.IsActive = model.IsActive ?? game.IsActive;
            game.DiscountPrice = model.DiscountPrice;
            
            game.DiscountPercentage = model.DiscountPercentage ?? 0;

            if (model.DeveloperId.HasValue)
            {
                var developer = await _context.Developers.FindAsync(model.DeveloperId.Value);
                if (developer != null)
                    game.Developer = developer;
            }

            if (model.PublisherId.HasValue)
            {
                var publisher = await _context.Publishers.FindAsync(model.PublisherId.Value);
                if (publisher != null)
                    game.Publisher = publisher;
            }

            if (model.GenreIds != null && model.GenreIds.Any())
            {
                // Clear existing genres
                game.Genres.Clear();

                // Add new genres
                var genres = await _context.Genres
                    .Where(g => model.GenreIds.Contains(g.Id))
                    .ToListAsync();
                
                foreach (var genre in genres)
                {
                    game.Genres.Add(genre);
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Game updated successfully" });
        }

        [HttpDelete("games/{id}")]
        public async Task<IActionResult> DeleteGame(int id)
        {
            var game = await _context.Games.FindAsync(id);
            if (game == null)
                return NotFound("Game not found");

            _context.Games.Remove(game);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Game deleted successfully" });
        }

        [HttpGet("games/{id}")]
        public async Task<IActionResult> GetGameDetails(int id)
        {
            var game = await _context.Games
                .Include(g => g.GameGenres).ThenInclude(gg => gg.Genre)
                .Include(g => g.GamePlatforms).ThenInclude(gp => gp.Platform)
                .Include(g => g.GameDevelopers).ThenInclude(gd => gd.Developer)
                .Include(g => g.GamePublishers).ThenInclude(gp => gp.Publisher)
                .Include(g => g.Screenshots)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (game == null)
                return NotFound("Game not found");

            return Ok(new
            {
                game.Id,
                game.Title,
                game.Description,
                game.Price,
                game.DiscountPercentage,
                game.ReleaseDate,
                game.CoverImageUrl,
                game.BackgroundImageUrl,
                game.TrailerUrl,
                game.IsFeatured,
                game.IsActive,
                game.MinimumSystemRequirements,
                game.RecommendedSystemRequirements,
                Screenshots = game.Screenshots.Select(s => s.Url).ToList(),
                Genres = game.GameGenres.Select(gg => new { id = gg.GenreId, name = gg.Genre.Name }).ToList(),
                Platforms = game.GamePlatforms.Select(gp => new { id = gp.PlatformId, name = gp.Platform.Name }).ToList(),
                Developers = game.GameDevelopers.Select(gd => new { id = gd.DeveloperId, name = gd.Developer.Name }).ToList(),
                Publishers = game.GamePublishers.Select(gp => new { id = gp.PublisherId, name = gp.Publisher.Name }).ToList()
            });
        }
    }

    public class UpdateOrderStatusModel
    {
        public string Status { get; set; }
    }

    public class AddGameModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public DateTime ReleaseDate { get; set; }
        public int DeveloperId { get; set; }
        public int PublisherId { get; set; }
        public string ImageUrl { get; set; }
        public string VideoUrl { get; set; }
        public List<int> GenreIds { get; set; }
        public string CoverImageUrl { get; set; }
        public string BackgroundImageUrl { get; set; }
        public string TrailerUrl { get; set; }
        public string MinimumSystemRequirements { get; set; }
        public string RecommendedSystemRequirements { get; set; }
        public bool IsFeatured { get; set; }
        public decimal DiscountPercentage { get; set; }
        public List<int> PlatformIds { get; set; }
        public List<string> Screenshots { get; set; }
    }

    public class UpdateGameModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal? Price { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public int? DeveloperId { get; set; }
        public int? PublisherId { get; set; }
        public string ImageUrl { get; set; }
        public string VideoUrl { get; set; }
        public bool? IsActive { get; set; }
        public decimal? DiscountPrice { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public List<int> GenreIds { get; set; }
    }
} 