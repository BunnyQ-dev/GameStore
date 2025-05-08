using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    [Route("api/games")]
    [ApiController]
    public class GamesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GamesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/games
        [HttpGet]
        public async Task<ActionResult<PagedResult<GameDto>>> GetGames(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 12, 
            [FromQuery] string? sort = "releaseDate_desc", 
            [FromQuery] string? genres = null, 
            [FromQuery] string? platforms = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] string? q = null // Додано для уніфікації з пошуком
        )
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            HashSet<int>? ownedGameIds = null;
            if (!string.IsNullOrEmpty(userId))
            {
                ownedGameIds = await _context.PurchasedGames
                    .Where(pg => pg.UserId == userId)
                    .Select(pg => pg.GameId)
                    .ToHashSetAsync();
            }

            IQueryable<Game> query = _context.Games.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q)) {
                query = query.Where(g => g.Title.Contains(q) || 
                            (g.Description != null && g.Description.Contains(q)) || 
                            g.GameGenres.Any(gg => gg.Genre.Name.Contains(q)) ||
                            g.GameDevelopers.Any(gd => gd.Developer.Name.Contains(q)) ||
                            g.GamePublishers.Any(gp => gp.Publisher.Name.Contains(q)));
            }
            if (!string.IsNullOrEmpty(genres))
            {
                var genreIds = genres.Split(',').Select(int.Parse).ToList();
                query = query.Where(g => g.GameGenres.Any(gg => genreIds.Contains(gg.GenreId)));
            }
            if (!string.IsNullOrEmpty(platforms))
            {
                var platformIds = platforms.Split(',').Select(int.Parse).ToList();
                query = query.Where(g => g.GamePlatforms.Any(gp => platformIds.Contains(gp.PlatformId)));
            }
             if (minPrice.HasValue)
            {
                query = query.Where(g => g.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                query = query.Where(g => g.Price <= maxPrice.Value);
            }

            query = sort switch
            {
                "price_asc" => query.OrderBy(g => g.Price),
                "price_desc" => query.OrderByDescending(g => g.Price),
                "name_asc" => query.OrderBy(g => g.Title),
                "name_desc" => query.OrderByDescending(g => g.Title),
                "releaseDate_asc" => query.OrderBy(g => g.ReleaseDate),
                _ => query.OrderByDescending(g => g.ReleaseDate) // Default: releaseDate_desc
            };

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var games = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(g => g.GameGenres).ThenInclude(gg => gg.Genre)
                .Include(g => g.GameDevelopers).ThenInclude(gd => gd.Developer)
                .Include(g => g.GamePublishers).ThenInclude(gp => gp.Publisher)
                .Include(g => g.GamePlatforms).ThenInclude(gp => gp.Platform)
                .Include(g => g.GameRatings)
                .ToListAsync();
            
            var gameDtos = games.Select(g => DtoMapper.MapGameToDto(g, ownedGameIds)).ToList();

            var result = new PagedResult<GameDto>
            {
                Items = gameDtos,
                TotalCount = totalCount,
                PageSize = pageSize,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return Ok(result);
        }

        // GET: api/games/new-releases
        [HttpGet("new-releases")]
        public async Task<ActionResult<IEnumerable<GameDto>>> GetNewReleases(int count = 6)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            HashSet<int>? ownedGameIds = null;
            if (!string.IsNullOrEmpty(userId))
            {
                ownedGameIds = await _context.PurchasedGames
                    .Where(pg => pg.UserId == userId)
                    .Select(pg => pg.GameId)
                    .ToHashSetAsync();
            }

            var games = await _context.Games
                .OrderByDescending(g => g.ReleaseDate)
                .Take(count)
                .Include(g => g.GameGenres).ThenInclude(gg => gg.Genre)
                .Include(g => g.GameDevelopers).ThenInclude(gd => gd.Developer)
                .Include(g => g.GamePublishers).ThenInclude(gp => gp.Publisher)
                .Include(g => g.GamePlatforms).ThenInclude(gp => gp.Platform) // Include necessary for mapper
                .Include(g => g.GameRatings) // Include necessary for mapper
                .ToListAsync(); // Fetch data first

            // Map in memory
            var gameDtos = games.Select(g => DtoMapper.MapGameToDto(g, ownedGameIds)).ToList();

            return Ok(gameDtos);
        }

        // GET: api/games/top-sellers
        [HttpGet("top-sellers")]
        public async Task<ActionResult<IEnumerable<GameDto>>> GetTopSellers(int count = 6)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            HashSet<int>? ownedGameIds = null;
            if (!string.IsNullOrEmpty(userId))
            {
                ownedGameIds = await _context.PurchasedGames
                    .Where(pg => pg.UserId == userId)
                    .Select(pg => pg.GameId)
                    .ToHashSetAsync();
            }

            var games = await _context.Games
                .OrderByDescending(g => g.Id)
                .Take(count)
                 .Include(g => g.GameGenres).ThenInclude(gg => gg.Genre)
                .Include(g => g.GameDevelopers).ThenInclude(gd => gd.Developer)
                .Include(g => g.GamePublishers).ThenInclude(gp => gp.Publisher)
                .Include(g => g.GamePlatforms).ThenInclude(gp => gp.Platform)
                .Include(g => g.GameRatings)
                .ToListAsync(); // Fetch data first

            // Map in memory
            var gameDtos = games.Select(g => DtoMapper.MapGameToDto(g, ownedGameIds)).ToList();

            return Ok(gameDtos);
        }

        // GET: api/games/special-offers
        [HttpGet("special-offers")]
        public async Task<ActionResult<IEnumerable<GameDto>>> GetSpecialOffers(int count = 6)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            HashSet<int>? ownedGameIds = null;
            if (!string.IsNullOrEmpty(userId))
            {
                 ownedGameIds = await _context.PurchasedGames.Where(pg => pg.UserId == userId).Select(pg => pg.GameId).ToHashSetAsync();
            }

            var games = await _context.Games
                .Where(g => g.DiscountPercentage > 0)
                .OrderByDescending(g => g.DiscountPercentage)
                .Take(count)
                .Include(g => g.GameGenres).ThenInclude(gg => gg.Genre)
                .Include(g => g.GameDevelopers).ThenInclude(gd => gd.Developer)
                .Include(g => g.GamePublishers).ThenInclude(gp => gp.Publisher)
                .Include(g => g.GamePlatforms).ThenInclude(gp => gp.Platform)
                .Include(g => g.GameRatings)
                .ToListAsync(); // Fetch first

            var gameDtos = games.Select(g => DtoMapper.MapGameToDto(g, ownedGameIds)).ToList(); // Map in memory
            // Original price handled by mapper
            return Ok(gameDtos);
        }

        // GET: api/games/featured
        [HttpGet("featured")]
        public async Task<ActionResult<IEnumerable<GameDto>>> GetFeaturedGames(int count = 6)
        {
             var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            HashSet<int>? ownedGameIds = null;
            if (!string.IsNullOrEmpty(userId))
            {
                 ownedGameIds = await _context.PurchasedGames.Where(pg => pg.UserId == userId).Select(pg => pg.GameId).ToHashSetAsync();
            }

            var games = await _context.Games
                .OrderByDescending(g => g.ReleaseDate)
                .Take(count)
                .Include(g => g.GameGenres).ThenInclude(gg => gg.Genre)
                .Include(g => g.GameDevelopers).ThenInclude(gd => gd.Developer)
                .Include(g => g.GamePublishers).ThenInclude(gp => gp.Publisher)
                .Include(g => g.GamePlatforms).ThenInclude(gp => gp.Platform)
                .Include(g => g.GameRatings)
                .ToListAsync(); // Fetch first

            var gameDtos = games.Select(g => DtoMapper.MapGameToDto(g, ownedGameIds)).ToList(); // Map in memory
            // Original price handled by mapper
            return Ok(gameDtos);
        }

        // GET: api/games/category/{categoryId}
        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<PagedResult<GameDto>>> GetGamesByCategory(int categoryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 12)
        {
            var category = await _context.Genres.FindAsync(categoryId);
            if (category == null) return NotFound("Категорію не знайдено");

             var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            HashSet<int>? ownedGameIds = null;
            if (!string.IsNullOrEmpty(userId))
            {
                 ownedGameIds = await _context.PurchasedGames.Where(pg => pg.UserId == userId).Select(pg => pg.GameId).ToHashSetAsync();
            }

            var query = _context.Games.Where(g => g.GameGenres.Any(gg => gg.GenreId == categoryId));
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var games = await query
                .OrderByDescending(g => g.ReleaseDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(g => g.GameGenres).ThenInclude(gg => gg.Genre)
                .Include(g => g.GameDevelopers).ThenInclude(gd => gd.Developer)
                .Include(g => g.GamePublishers).ThenInclude(gp => gp.Publisher)
                .Include(g => g.GamePlatforms).ThenInclude(gp => gp.Platform)
                .Include(g => g.GameRatings)
                .ToListAsync(); // Fetch first

            var gameDtos = games.Select(g => DtoMapper.MapGameToDto(g, ownedGameIds)).ToList(); // Map in memory
            // Original price handled by mapper

             var result = new PagedResult<GameDto>
            {
                Items = gameDtos,
                TotalCount = totalCount,
                PageSize = pageSize,
                CurrentPage = page,
                TotalPages = totalPages,
            };

            return Ok(result);
        }

        // GET: api/games/search
        [HttpGet("search")]
        public async Task<ActionResult<PagedResult<GameDto>>> SearchGames([FromQuery] string q, [FromQuery] int page = 1, [FromQuery] int pageSize = 12)
        {
            if (string.IsNullOrWhiteSpace(q)) return BadRequest("Пошуковий запит не може бути порожнім");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            HashSet<int>? ownedGameIds = null;
            if (!string.IsNullOrEmpty(userId))
            {
                 ownedGameIds = await _context.PurchasedGames.Where(pg => pg.UserId == userId).Select(pg => pg.GameId).ToHashSetAsync();
            }

            var query = _context.Games
                 .Where(g => g.Title.Contains(q) || 
                            (g.Description != null && g.Description.Contains(q)) || 
                            g.GameGenres.Any(gg => gg.Genre.Name.Contains(q)) ||
                            g.GameDevelopers.Any(gd => gd.Developer.Name.Contains(q)) ||
                            g.GamePublishers.Any(gp => gp.Publisher.Name.Contains(q)));

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var games = await query
                .OrderByDescending(g => g.ReleaseDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                 .Include(g => g.GameGenres).ThenInclude(gg => gg.Genre)
                .Include(g => g.GameDevelopers).ThenInclude(gd => gd.Developer)
                .Include(g => g.GamePublishers).ThenInclude(gp => gp.Publisher)
                .Include(g => g.GamePlatforms).ThenInclude(gp => gp.Platform)
                .Include(g => g.GameRatings)
                .ToListAsync(); // Fetch first

            var gameDtos = games.Select(g => DtoMapper.MapGameToDto(g, ownedGameIds)).ToList(); // Map in memory
            // Original price handled by mapper

             var result = new PagedResult<GameDto>
            {
                Items = gameDtos,
                TotalCount = totalCount,
                PageSize = pageSize,
                CurrentPage = page,
                TotalPages = totalPages,
            };

            return Ok(result);
        }

        // GET: api/games/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<GameDetailsDto>> GetGame(int id)
        {
            var game = await _context.Games
                .Include(g => g.GameGenres).ThenInclude(gg => gg.Genre)
                .Include(g => g.GamePlatforms).ThenInclude(gp => gp.Platform)
                .Include(g => g.GameDevelopers).ThenInclude(gd => gd.Developer)
                .Include(g => g.GamePublishers).ThenInclude(gp => gp.Publisher)
                .Include(g => g.Screenshots)
                .Include(g => g.GameRatings).ThenInclude(gr => gr.User) // Include User for UserName in RatingDto
                .FirstOrDefaultAsync(g => g.Id == id);

            if (game == null) return NotFound();

            bool isInWishlist = false;
            bool isOwned = false;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                isInWishlist = await _context.Wishlists.AnyAsync(w => w.GameId == id && w.UserId == userId);
                isOwned = await _context.PurchasedGames.AnyAsync(pg => pg.GameId == id && pg.UserId == userId);
            }
            // Use the updated DtoMapper method
            var gameDetailsDto = DtoMapper.MapGameDetailsToDto(game, isOwned, isInWishlist);

            if(gameDetailsDto == null) {
                // Handle potential mapping error, though unlikely if game is not null
                 return Problem("Error mapping game details.");
            }
            
            return Ok(gameDetailsDto);
        }

        // POST: api/Games
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<GameDto>> CreateGame([FromBody] AdminGameDto dto)
        {
            var game = new Game
            {
                Title = dto.Title,
                Description = dto.Description,
                ReleaseDate = dto.ReleaseDate,
                Price = dto.Price,
                CoverImageUrl = dto.CoverImageUrl,
                TrailerUrl = dto.TrailerUrl,
                IsActive = dto.IsActive,
                IsFeatured = dto.IsFeatured,
                BackgroundImageUrl = dto.BackgroundImageUrl,
                MinimumSystemRequirements = dto.MinimumSystemRequirements,
                RecommendedSystemRequirements = dto.RecommendedSystemRequirements,
                DiscountPercentage = dto.DiscountPercentage
            };
            
            _context.Games.Add(game);
            await _context.SaveChangesAsync();
            
            // Developers
            if (dto.DeveloperIds != null)
            {
                foreach (var devId in dto.DeveloperIds)
                {
                    _context.GameDevelopers.Add(new GameDeveloper { GameId = game.Id, DeveloperId = devId });
                }
            }
            
            // Publishers
            if (dto.PublisherIds != null)
            {
                foreach (var pubId in dto.PublisherIds)
                {
                    _context.GamePublishers.Add(new GamePublisher { GameId = game.Id, PublisherId = pubId });
                }
            }
            
            // Genres
            if (dto.GenreIds != null)
            {
                foreach (var genreId in dto.GenreIds)
                {
                    _context.GameGenres.Add(new GameGenre { GameId = game.Id, GenreId = genreId });
                }
            }
            
            // Platforms
            if (dto.PlatformIds != null)
            {
                foreach (var platformId in dto.PlatformIds)
                {
                    _context.GamePlatforms.Add(new GamePlatform { GameId = game.Id, PlatformId = platformId });
                }
            }
            
            // Screenshots
            if (dto.Screenshots != null)
            {
                foreach (var url in dto.Screenshots)
                {
                    _context.Screenshots.Add(new Screenshot { GameId = game.Id, Url = url });
                }
            }
            
            await _context.SaveChangesAsync();
            
            return CreatedAtAction(nameof(GetGame), new { id = game.Id }, new { game.Id, game.Title, message = "Гру успішно додано" });
        }

        // PUT: api/Games/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateGame(int id, [FromBody] AdminGameDto dto)
        {
            if (id != dto.Id) return BadRequest();
            
            var game = await _context.Games
                .Include(g => g.GameDevelopers)
                .Include(g => g.GamePublishers)
                .Include(g => g.GameGenres)
                .Include(g => g.GamePlatforms)
                .Include(g => g.Screenshots)
                .FirstOrDefaultAsync(g => g.Id == id);
                
            if (game == null) return NotFound();
            
            game.Title = dto.Title;
            game.Description = dto.Description;
            game.ReleaseDate = dto.ReleaseDate;
            game.Price = dto.Price;
            game.CoverImageUrl = dto.CoverImageUrl;
            game.TrailerUrl = dto.TrailerUrl;
            game.IsActive = dto.IsActive;
            game.IsFeatured = dto.IsFeatured;
            game.BackgroundImageUrl = dto.BackgroundImageUrl;
            game.MinimumSystemRequirements = dto.MinimumSystemRequirements;
            game.RecommendedSystemRequirements = dto.RecommendedSystemRequirements;
            game.DiscountPercentage = dto.DiscountPercentage;
            
            // Update developers
            game.GameDevelopers.Clear();
            if (dto.DeveloperIds != null)
            {
                foreach (var devId in dto.DeveloperIds)
                {
                    game.GameDevelopers.Add(new GameDeveloper { GameId = id, DeveloperId = devId });
                }
            }
            
            // Update publishers
            game.GamePublishers.Clear();
            if (dto.PublisherIds != null)
            {
                foreach (var pubId in dto.PublisherIds)
                {
                    game.GamePublishers.Add(new GamePublisher { GameId = id, PublisherId = pubId });
                }
            }
            
            // Update genres
            game.GameGenres.Clear();
            if (dto.GenreIds != null)
            {
                foreach (var genreId in dto.GenreIds)
                {
                    game.GameGenres.Add(new GameGenre { GameId = id, GenreId = genreId });
                }
            }
            
            // Update platforms
            game.GamePlatforms.Clear();
            if (dto.PlatformIds != null)
            {
                foreach (var platformId in dto.PlatformIds)
                {
                    game.GamePlatforms.Add(new GamePlatform { GameId = id, PlatformId = platformId });
                }
            }
            
            // Update screenshots
            if (dto.Screenshots != null)
            {
                var oldScreenshots = await _context.Screenshots.Where(s => s.GameId == id).ToListAsync();
                _context.Screenshots.RemoveRange(oldScreenshots);

                foreach (var url in dto.Screenshots)
                {
                    _context.Screenshots.Add(new Screenshot { GameId = id, Url = url });
                }
            }
            
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Games/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteGame(int id)
        {
            var game = await _context.Games.FindAsync(id);
            if (game == null)
            {
                return NotFound();
            }

            _context.Games.Remove(game);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Games/ByGenre/5
        [HttpGet("ByGenre/{genreId}")]
        public async Task<ActionResult<IEnumerable<GameDto>>> GetGamesByGenre(int genreId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            HashSet<int>? ownedGameIds = null;
            if (!string.IsNullOrEmpty(userId))
            {
                 ownedGameIds = await _context.PurchasedGames.Where(pg => pg.UserId == userId).Select(pg => pg.GameId).ToHashSetAsync();
            }

            var games = await _context.Games
                .Where(g => g.GameGenres.Any(gg => gg.GenreId == genreId))
                .Include(g => g.GameGenres).ThenInclude(gg => gg.Genre)
                .Include(g => g.GameDevelopers).ThenInclude(gd => gd.Developer)
                .Include(g => g.GamePublishers).ThenInclude(gp => gp.Publisher)
                .Include(g => g.GamePlatforms).ThenInclude(gp => gp.Platform)
                .Include(g => g.GameRatings)
                .ToListAsync(); // Fetch first

            if (!games.Any())
            {
                return NotFound($"Не знайдено ігор для жанру з ID {genreId}.");
            }

            var gameDtos = games.Select(g => DtoMapper.MapGameToDto(g, ownedGameIds)).ToList(); // Map in memory

            return Ok(gameDtos);
        }

        // GET: api/games/ByPlatform/{platformId}
        [HttpGet("ByPlatform/{platformId}")]
        public async Task<ActionResult<IEnumerable<GameDto>>> GetGamesByPlatform(int platformId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            HashSet<int>? ownedGameIds = null;
            if (!string.IsNullOrEmpty(userId))
            {
                 ownedGameIds = await _context.PurchasedGames.Where(pg => pg.UserId == userId).Select(pg => pg.GameId).ToHashSetAsync();
            }

            var games = await _context.Games
                .Where(g => g.GamePlatforms.Any(gp => gp.PlatformId == platformId))
                 .Include(g => g.GameGenres).ThenInclude(gg => gg.Genre)
                .Include(g => g.GameDevelopers).ThenInclude(gd => gd.Developer)
                .Include(g => g.GamePublishers).ThenInclude(gp => gp.Publisher)
                .Include(g => g.GamePlatforms).ThenInclude(gp => gp.Platform)
                .Include(g => g.GameRatings)
                .ToListAsync(); // Fetch first

            if (!games.Any())
            {
                return NotFound($"Не знайдено ігор для платформи з ID {platformId}.");
            }
            
            var gameDtos = games.Select(g => DtoMapper.MapGameToDto(g, ownedGameIds)).ToList(); // Map in memory

            return Ok(gameDtos);
        }

        // POST: api/games/AddGenre
        [HttpPost("AddGenre")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddGenreToGame(int gameId, int genreId)
        {
            var game = await _context.Games.FindAsync(gameId);
            if (game == null)
            {
                return NotFound("Game not found");
            }

            var genre = await _context.Genres.FindAsync(genreId);
            if (genre == null)
            {
                return NotFound("Genre not found");
            }

            var gameGenre = new GameGenre
            {
                GameId = gameId,
                GenreId = genreId
            };

            _context.GameGenres.Add(gameGenre);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (await _context.GameGenres.AnyAsync(gg => gg.GameId == gameId && gg.GenreId == genreId))
                {
                    return Conflict("Game-Genre relationship already exists");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Games/RemoveGenre
        [HttpDelete("RemoveGenre")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveGenreFromGame(int gameId, int genreId)
        {
            var gameGenre = await _context.GameGenres
                .FirstOrDefaultAsync(gg => gg.GameId == gameId && gg.GenreId == genreId);

            if (gameGenre == null)
            {
                return NotFound("Game-Genre relationship not found");
            }

            _context.GameGenres.Remove(gameGenre);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        // GET: api/games/{gameId}/reviews
        [HttpGet("{gameId}/reviews")]
        public async Task<ActionResult<IEnumerable<GameReviewDto>>> GetGameReviews(int gameId)
        {
            var reviews = await _context.GameRatings
                .Where(r => r.GameId == gameId)
                .Include(r => r.User)
                .OrderByDescending(r => r.LastUpdated ?? r.RatingDate)
                .Select(r => new GameReviewDto
                {
                    UserId = r.UserId,
                    UserName = r.User.UserName ?? "Unknown",
                    UserDisplayName = r.User.DisplayName,
                    UserAvatarUrl = r.User.ProfilePictureUrl,
                    GameId = r.GameId,
                    Rating = r.Score,
                    Comment = r.Comment,
                    CreatedAt = r.RatingDate,
                    UpdatedAt = r.LastUpdated ?? r.RatingDate,
                    IsCurrentUserReview = false
                })
                .ToListAsync();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(currentUserId))
            {
                foreach (var review in reviews)
                {
                    if (review.UserId == currentUserId)
                    {
                        review.IsCurrentUserReview = true;
                    }
                }
            }

            return Ok(reviews);
        }

        // GET: api/games/{gameId}/reviews/my
        [HttpGet("{gameId}/reviews/my")]
        [Authorize]
        public async Task<ActionResult<GameReviewDto>> GetMyGameReview(int gameId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var review = await _context.GameRatings
                .Where(r => r.GameId == gameId && r.UserId == userId)
                .Include(r => r.User)
                .Select(r => new GameReviewDto
                {
                    UserId = r.UserId,
                    UserName = r.User.UserName ?? "Unknown",
                    UserDisplayName = r.User.DisplayName,
                    UserAvatarUrl = r.User.ProfilePictureUrl,
                    GameId = r.GameId,
                    Rating = r.Score,
                    Comment = r.Comment,
                    CreatedAt = r.RatingDate,
                    UpdatedAt = r.LastUpdated ?? r.RatingDate,
                    IsCurrentUserReview = true
                })
                .FirstOrDefaultAsync();

            if (review == null)
            {
                return NotFound("Ви ще не залишили відгук для цієї гри.");
            }

            return Ok(review);
        }

        // POST: api/games/{gameId}/reviews
        [HttpPost("{gameId}/reviews")]
        [Authorize]
        public async Task<IActionResult> PostGameReview(int gameId, [FromBody] GameReviewInputDto reviewDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var gameExists = await _context.Games.AnyAsync(g => g.Id == gameId);
            if (!gameExists) return NotFound("Гру не знайдено.");

            if (reviewDto.Rating < 1 || reviewDto.Rating > 5) 
            {
                return BadRequest("Рейтинг має бути від 1 до 5.");
            }

            var existingReview = await _context.GameRatings
                .FirstOrDefaultAsync(r => r.GameId == gameId && r.UserId == userId);

            if (existingReview != null)
            {
                existingReview.Score = reviewDto.Rating;
                existingReview.Comment = reviewDto.Comment;
                existingReview.LastUpdated = DateTime.UtcNow;
                _context.Entry(existingReview).State = EntityState.Modified;
            }
            else
            {
                var newReview = new GameRating
                {
                    UserId = userId,
                    GameId = gameId,
                    Score = reviewDto.Rating,
                    Comment = reviewDto.Comment,
                    RatingDate = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                };
                _context.GameRatings.Add(newReview);
            }

            try
            {
                await _context.SaveChangesAsync();
                await UpdateGameAverageRatingAsync(gameId);
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, "Помилка збереження відгуку: " + ex.Message);
            }

            return Ok(new { message = "Відгук успішно збережено." });
        }

        // DELETE: api/games/{gameId}/reviews
        [HttpDelete("{gameId}/reviews")]
        [Authorize]
        public async Task<IActionResult> DeleteGameReview(int gameId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var reviewToDelete = await _context.GameRatings
                .FirstOrDefaultAsync(r => r.GameId == gameId && r.UserId == userId);

            if (reviewToDelete == null)
            {
                return NotFound("Ваш відгук для цієї гри не знайдено.");
            }

            _context.GameRatings.Remove(reviewToDelete);
            await _context.SaveChangesAsync();

            await UpdateGameAverageRatingAsync(gameId);

            return NoContent();
        }


        private async Task UpdateGameAverageRatingAsync(int gameId)
        {
            var game = await _context.Games.FindAsync(gameId);
            if (game == null) return;

            var reviews = await _context.GameRatings
                .Where(r => r.GameId == gameId)
                .ToListAsync();

            if (reviews.Any())
            {
                game.ReviewCount = reviews.Count;
                game.AverageRating = reviews.Average(r => r.Score);
            }
            else
            {
                game.ReviewCount = 0;
                game.AverageRating = null;
            }

            _context.Entry(game).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        private bool GameExists(int id)
        {
            return _context.Games.Any(e => e.Id == id);
        }
    }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int PageSize { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
} 