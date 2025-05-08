using Games_Store.Data;
using Games_Store.DTOs;
using Games_Store.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System;

namespace Games_Store.Controllers
{
    [Route("api/admin/manage")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminManagementController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminManagementController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // --- (Genres) ---

        [HttpGet("genres")]
        public async Task<ActionResult<IEnumerable<GenreDto>>> GetGenres()
        {
            var genres = await _context.Genres
                .OrderBy(g => g.Name)
                .Select(g => new GenreDto { Id = g.Id, Name = g.Name })
                .ToListAsync();
            return Ok(genres);
        }

        [HttpPost("genres")]
        public async Task<ActionResult<GenreDto>> CreateGenre([FromBody] CreateOrUpdateGenreDto genreDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (await _context.Genres.AnyAsync(g => g.Name == genreDto.Name))
            {
                return Conflict(new { message = "Genre with this name already exists." });
            }

            var genre = new Genre { Name = genreDto.Name };
            _context.Genres.Add(genre);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetGenres), new { }, new GenreDto { Id = genre.Id, Name = genre.Name });
        }

        [HttpPut("genres/{id}")]
        public async Task<IActionResult> UpdateGenre(int id, [FromBody] CreateOrUpdateGenreDto genreDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var genre = await _context.Genres.FindAsync(id);
            if (genre == null)
            {
                return NotFound(new { message = "Genre not found." });
            }

            if (await _context.Genres.AnyAsync(g => g.Name == genreDto.Name && g.Id != id))
            {
                return Conflict(new { message = "Genre with this name already exists." });
            }

            genre.Name = genreDto.Name;
            _context.Entry(genre).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                 if (!await _context.Genres.AnyAsync(e => e.Id == id)) { return NotFound(); } else { throw; }
            }

            return NoContent();
        }

        [HttpDelete("genres/{id}")]
        public async Task<IActionResult> DeleteGenre(int id)
        {
            var genre = await _context.Genres.FindAsync(id);
            if (genre == null)
            {
                return NotFound(new { message = "Genre not found." });
            }

            var isUsed = await _context.GameGenres.AnyAsync(gg => gg.GenreId == id);
            if (isUsed)
            {
                 return Conflict(new { message = "Cannot delete genre because it is used in games." });
            }

            _context.Genres.Remove(genre);
            await _context.SaveChangesAsync();

            return NoContent(); // Успішне видалення
        }

        // --- (Developers) ---

        [HttpGet("developers")]
        public async Task<ActionResult<IEnumerable<DeveloperDto>>> GetDevelopers()
        {
            var developers = await _context.Developers
                .OrderBy(d => d.Name)
                .Select(d => new DeveloperDto { Id = d.Id, Name = d.Name })
                .ToListAsync();
            return Ok(developers);
        }

        [HttpPost("developers")]
        public async Task<ActionResult<DeveloperDto>> CreateDeveloper([FromBody] CreateOrUpdateDeveloperDto developerDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (await _context.Developers.AnyAsync(d => d.Name == developerDto.Name))
            {
                return Conflict(new { message = "Developer with this name already exists." });
            }

            var developer = new Developer { Name = developerDto.Name };
            _context.Developers.Add(developer);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDevelopers), new { }, new DeveloperDto { Id = developer.Id, Name = developer.Name });
        }

        [HttpPut("developers/{id}")]
        public async Task<IActionResult> UpdateDeveloper(int id, [FromBody] CreateOrUpdateDeveloperDto developerDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var developer = await _context.Developers.FindAsync(id);
            if (developer == null) return NotFound(new { message = "Developer not found." });

            if (await _context.Developers.AnyAsync(d => d.Name == developerDto.Name && d.Id != id))
            {
                return Conflict(new { message = "Developer with this name already exists." });
            }

            developer.Name = developerDto.Name;
            _context.Entry(developer).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); } catch (DbUpdateConcurrencyException) { if (!await _context.Developers.AnyAsync(e => e.Id == id)) { return NotFound(); } else { throw; } }
            return NoContent();
        }

        [HttpDelete("developers/{id}")]
        public async Task<IActionResult> DeleteDeveloper(int id)
        {
            var developer = await _context.Developers.FindAsync(id);
            if (developer == null) return NotFound(new { message = "Developer not found." });

            var isUsed = await _context.Games.AnyAsync(g => g.Developer != null && g.Developer.Id == id);
            if (isUsed) return Conflict(new { message = "Cannot delete developer because it is used in games." });

            _context.Developers.Remove(developer);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // --- (Publishers) ---

        [HttpGet("publishers")]
        public async Task<ActionResult<IEnumerable<PublisherDto>>> GetPublishers()
        {
            var publishers = await _context.Publishers
                .OrderBy(p => p.Name)
                .Select(p => new PublisherDto { Id = p.Id, Name = p.Name })
                .ToListAsync();
            return Ok(publishers);
        }

        [HttpPost("publishers")]
        public async Task<ActionResult<PublisherDto>> CreatePublisher([FromBody] CreateOrUpdatePublisherDto publisherDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (await _context.Publishers.AnyAsync(p => p.Name == publisherDto.Name))
            {
                return Conflict(new { message = "Publisher with this name already exists." });
            }

            var publisher = new Publisher { Name = publisherDto.Name };
            _context.Publishers.Add(publisher);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPublishers), new { }, new PublisherDto { Id = publisher.Id, Name = publisher.Name });
        }

        [HttpPut("publishers/{id}")]
        public async Task<IActionResult> UpdatePublisher(int id, [FromBody] CreateOrUpdatePublisherDto publisherDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var publisher = await _context.Publishers.FindAsync(id);
            if (publisher == null) return NotFound(new { message = "Publisher not found." });

            if (await _context.Publishers.AnyAsync(p => p.Name == publisherDto.Name && p.Id != id))
            {
                return Conflict(new { message = "Publisher with this name already exists." });
            }

            publisher.Name = publisherDto.Name;
            _context.Entry(publisher).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); } catch (DbUpdateConcurrencyException) { if (!await _context.Publishers.AnyAsync(e => e.Id == id)) { return NotFound(); } else { throw; } }
            return NoContent();
        }

        [HttpDelete("publishers/{id}")]
        public async Task<IActionResult> DeletePublisher(int id)
        {
            var publisher = await _context.Publishers.FindAsync(id);
            if (publisher == null) return NotFound(new { message = "Publisher not found." });

            var isUsed = await _context.Games.AnyAsync(g => g.Publisher != null && g.Publisher.Id == id);
            if (isUsed) return Conflict(new { message = "Cannot delete publisher because it is used in games." });

            _context.Publishers.Remove(publisher);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // --- (Platforms) ---

        [HttpGet("platforms")]
        public async Task<ActionResult<IEnumerable<PlatformDto>>> GetPlatforms()
        {
            var platforms = await _context.Platforms
                .OrderBy(p => p.Name)
                .Select(p => new PlatformDto { Id = p.Id, Name = p.Name })
                .ToListAsync();
            return Ok(platforms);
        }

        [HttpPost("platforms")]
        public async Task<ActionResult<PlatformDto>> CreatePlatform([FromBody] CreateOrUpdatePlatformDto platformDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (await _context.Platforms.AnyAsync(p => p.Name == platformDto.Name))
                return Conflict(new { message = "Platform with this name already exists." });

            var platform = new Platform { Name = platformDto.Name };
            _context.Platforms.Add(platform);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPlatforms), new { }, new PlatformDto { Id = platform.Id, Name = platform.Name });
        }

        [HttpPut("platforms/{id}")]
        public async Task<IActionResult> UpdatePlatform(int id, [FromBody] CreateOrUpdatePlatformDto platformDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var platform = await _context.Platforms.FindAsync(id);
            if (platform == null) return NotFound(new { message = "Platform not found." });

            if (await _context.Platforms.AnyAsync(p => p.Name == platformDto.Name && p.Id != id))
                return Conflict(new { message = "Platform with this name already exists." });

            platform.Name = platformDto.Name;
            _context.Entry(platform).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Platforms.AnyAsync(p => p.Id == id)) return NotFound(); else throw;
            }

            return NoContent();
        }

        [HttpDelete("platforms/{id}")]
        public async Task<IActionResult> DeletePlatform(int id)
        {
            var platform = await _context.Platforms.FindAsync(id);
            if (platform == null) return NotFound(new { message = "Platform not found." });

            var isUsed = await _context.GamePlatforms.AnyAsync(gp => gp.PlatformId == id);
            if (isUsed) return Conflict(new { message = "Cannot delete platform because it is used in games." });

            _context.Platforms.Remove(platform);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // --- (Games) ---

        [HttpGet("games/{id}")]
        public async Task<ActionResult<AdminGameDetailsDto>> GetGameDetails(int id)
        {
            var game = await _context.Games
                .Include(g => g.GameGenres).ThenInclude(gg => gg.Genre)
                .Include(g => g.GamePlatforms).ThenInclude(gp => gp.Platform)
                .Include(g => g.GameDevelopers).ThenInclude(gd => gd.Developer)
                .Include(g => g.GamePublishers).ThenInclude(gp => gp.Publisher)
                .Include(g => g.Screenshots)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (game == null) return NotFound(new { message = $"Game with id {id} not found" });

            var gameDto = new AdminGameDetailsDto
            {
                Id = game.Id,
                Title = game.Title,
                Description = game.Description,
                Price = game.Price,
                DiscountPercentage = game.DiscountPercentage,
                DiscountPrice = game.DiscountPrice,
                ReleaseDate = game.ReleaseDate,
                IsActive = game.IsActive,
                IsFeatured = game.IsFeatured,
                DeveloperId = game.GameDevelopers.FirstOrDefault()?.DeveloperId,
                PublisherId = game.GamePublishers.FirstOrDefault()?.PublisherId,
                Genres = game.GameGenres.Select(gg => new GenreDto { Id = gg.GenreId, Name = gg.Genre.Name }).ToList(),
                Platforms = game.GamePlatforms.Select(gp => new PlatformDto { Id = gp.PlatformId, Name = gp.Platform.Name }).ToList(),
                GenreIds = game.GameGenres.Select(gg => gg.GenreId).ToList(),
                PlatformIds = game.GamePlatforms.Select(gp => gp.PlatformId).ToList(),
                CoverImageUrl = game.CoverImageUrl,
                BackgroundImageUrl = game.BackgroundImageUrl,
                TrailerUrl = game.TrailerUrl,
                MinimumSystemRequirements = game.MinimumSystemRequirements,
                RecommendedSystemRequirements = game.RecommendedSystemRequirements,
                ScreenshotUrls = game.Screenshots.Select(s => s.Url).ToList()
            };
            return Ok(gameDto);
        }

        [HttpPut("games/{id}")]
        public async Task<IActionResult> UpdateGame(int id, [FromBody] AdminGameDetailsDto gameDto)
        {
            if (id != gameDto.Id) return BadRequest("ID mismatch");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var gameToUpdate = await _context.Games
                .Include(g => g.GameGenres)
                .Include(g => g.GamePlatforms)
                .Include(g => g.GameDevelopers)
                .Include(g => g.GamePublishers)
                .Include(g => g.Screenshots)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (gameToUpdate == null) return NotFound(new { message = $"Game with id {id} not found" });

            gameToUpdate.Title = gameDto.Title;
            gameToUpdate.Description = gameDto.Description;
            gameToUpdate.Price = gameDto.Price;
            gameToUpdate.DiscountPercentage = gameDto.DiscountPercentage;
            gameToUpdate.DiscountPrice = gameDto.DiscountPrice;
            
            if (gameDto.ReleaseDate.HasValue)
            {
                gameToUpdate.ReleaseDate = gameDto.ReleaseDate.Value;
            }
            
            gameToUpdate.IsActive = gameDto.IsActive;
            gameToUpdate.IsFeatured = gameDto.IsFeatured;
            gameToUpdate.CoverImageUrl = gameDto.CoverImageUrl;
            gameToUpdate.BackgroundImageUrl = gameDto.BackgroundImageUrl;
            gameToUpdate.TrailerUrl = gameDto.TrailerUrl;
            gameToUpdate.MinimumSystemRequirements = gameDto.MinimumSystemRequirements;
            gameToUpdate.RecommendedSystemRequirements = gameDto.RecommendedSystemRequirements;

            if (gameDto.DeveloperId.HasValue) {
                var currentDevLink = gameToUpdate.GameDevelopers.FirstOrDefault();
                if (currentDevLink == null || currentDevLink.DeveloperId != gameDto.DeveloperId.Value) {
                    if (currentDevLink != null) _context.GameDevelopers.Remove(currentDevLink);
                    _context.GameDevelopers.Add(new GameDeveloper { GameId = id, DeveloperId = gameDto.DeveloperId.Value });
                }
            } else { 
                var currentDevLink = gameToUpdate.GameDevelopers.FirstOrDefault();
                if (currentDevLink != null) _context.GameDevelopers.Remove(currentDevLink);
            }

            if (gameDto.PublisherId.HasValue) {
                var currentPubLink = gameToUpdate.GamePublishers.FirstOrDefault();
                if (currentPubLink == null || currentPubLink.PublisherId != gameDto.PublisherId.Value) {
                    if (currentPubLink != null) _context.GamePublishers.Remove(currentPubLink);
                    _context.GamePublishers.Add(new GamePublisher { GameId = id, PublisherId = gameDto.PublisherId.Value });
                }
            } else { 
                var currentPubLink = gameToUpdate.GamePublishers.FirstOrDefault();
                if (currentPubLink != null) _context.GamePublishers.Remove(currentPubLink);
            }

            // Оновлення GenreIds
            if (gameDto.GenreIds != null) {
                var genresToRemove = gameToUpdate.GameGenres.Where(gg => !gameDto.GenreIds.Contains(gg.GenreId)).ToList();
                _context.GameGenres.RemoveRange(genresToRemove);
                var existingGenreIds = gameToUpdate.GameGenres.Select(gg => gg.GenreId).ToList();
                var genresToAdd = gameDto.GenreIds.Where(newId => !existingGenreIds.Contains(newId))
                                    .Select(newId => new GameGenre { GameId = id, GenreId = newId }).ToList();
                await _context.GameGenres.AddRangeAsync(genresToAdd);
            }

            if (gameDto.PlatformIds != null) {
                var platformsToRemove = gameToUpdate.GamePlatforms.Where(gp => !gameDto.PlatformIds.Contains(gp.PlatformId)).ToList();
                _context.GamePlatforms.RemoveRange(platformsToRemove);
                var existingPlatformIds = gameToUpdate.GamePlatforms.Select(gp => gp.PlatformId).ToList();
                var platformsToAdd = gameDto.PlatformIds.Where(newId => !existingPlatformIds.Contains(newId))
                                    .Select(newId => new GamePlatform { GameId = id, PlatformId = newId }).ToList();
                await _context.GamePlatforms.AddRangeAsync(platformsToAdd);
            }

            if (gameDto.ScreenshotUrls != null)
            {
                _context.Screenshots.RemoveRange(gameToUpdate.Screenshots);
                if (gameDto.ScreenshotUrls.Any())
                {
                    gameToUpdate.Screenshots = gameDto.ScreenshotUrls.Select(url => new Screenshot { Url = url, GameId = id }).ToList();
                }
            }

            try {
                await _context.SaveChangesAsync();
            } catch (DbUpdateConcurrencyException) {
                if (!await _context.Games.AnyAsync(e => e.Id == id)) { return NotFound(); }
                else { throw; }
            }
            return NoContent();
        }

        // --- (Achievements) ---

        [HttpGet("achievements")]
        public async Task<ActionResult<IEnumerable<AchievementDto>>> GetAchievements()
        {
            var achievements = await _context.Achievements
                .OrderBy(a => a.Name)
                .Select(a => new AchievementDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    Description = a.Description,
                    IconUrl = a.IconUrl

                })
                .ToListAsync();
            return Ok(achievements);
        }

        [HttpPost("achievements")]
        public async Task<ActionResult<AchievementDto>> CreateAchievement([FromBody] CreateOrUpdateAchievementDto achievementDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (!await _context.Games.AnyAsync(g => g.Id == achievementDto.GameId)) return NotFound(new { message = "Game not found"});

            var achievement = new Achievement
            {
                Name = achievementDto.Name,
                Description = achievementDto.Description,
                IconUrl = achievementDto.IconUrl,
                GameId = achievementDto.GameId
            };
            _context.Achievements.Add(achievement);
            await _context.SaveChangesAsync();

            var resultDto = new AchievementDto
            {
                Id = achievement.Id,
                Name = achievement.Name,
                Description = achievement.Description,
                IconUrl = achievement.IconUrl,
                GameId = achievement.GameId
            };
            return CreatedAtAction(nameof(GetAchievements), new { }, resultDto);
        }

        [HttpPut("achievements/{id}")]
        public async Task<IActionResult> UpdateAchievement(int id, [FromBody] CreateOrUpdateAchievementDto achievementDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var achievement = await _context.Achievements.FindAsync(id);
            if (achievement == null) return NotFound(new { message = "Achievement not found." });

            // Перевірка гри, якщо потрібно
            if (achievementDto.GameId > 0 && !await _context.Games.AnyAsync(g => g.Id == achievementDto.GameId)) return NotFound(new { message = "Game not found"});

            achievement.Name = achievementDto.Name;
            achievement.Description = achievementDto.Description;
            achievement.IconUrl = achievementDto.IconUrl;
            achievement.GameId = achievementDto.GameId;

            _context.Entry(achievement).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); } catch (DbUpdateConcurrencyException) { if (!await _context.Achievements.AnyAsync(e => e.Id == id)) { return NotFound(); } else { throw; } }
            return NoContent();
        }

        [HttpDelete("achievements/{id}")]
        public async Task<IActionResult> DeleteAchievement(int id)
        {
            var achievement = await _context.Achievements.FindAsync(id);
            if (achievement == null) return NotFound(new { message = "Achievement not found." });

            // Перевірка, чи досягнення використовується (наприклад, у UserAchievements)
            var isUsed = await _context.UserAchievements.AnyAsync(ua => ua.AchievementId == id);
            if (isUsed) return Conflict(new { message = "Cannot delete achievement because it has been obtained by users." });

            _context.Achievements.Remove(achievement);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // --- (Users) ---

        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<UserAdminDto>>> GetUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var list = new List<UserAdminDto>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                list.Add(new UserAdminDto {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Roles = roles,
                    IsActive = user.IsActive
                });
            }
            return Ok(list);
        }

        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UserAdminUpdateDto dto)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound(new { message = "User not found." });

            user.IsActive = dto.IsActive;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded) return BadRequest(updateResult.Errors);

            var currentRoles = await _userManager.GetRolesAsync(user);
            var rolesToAdd = dto.Roles.Except(currentRoles).ToList();
            var rolesToRemove = currentRoles.Except(dto.Roles).ToList();
            if (rolesToAdd.Any())
            {
                var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                if (!addResult.Succeeded) return BadRequest(addResult.Errors);
            }
            if (rolesToRemove.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeResult.Succeeded) return BadRequest(removeResult.Errors);
            }

            return NoContent();
        }

        [HttpPost("users/{id}/achievements")]
        public async Task<IActionResult> AddUserAchievement(string id, [FromBody] AddUserAchievementDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound(new { message = "User not found." });

            if (!await _context.Achievements.AnyAsync(a => a.Id == dto.AchievementId))
                return NotFound(new { message = "Achievement not found." });

            if (await _context.UserAchievements.AnyAsync(ua => ua.UserId == id && ua.AchievementId == dto.AchievementId))
                return Conflict(new { message = "User already has this achievement." });

            var userAchievement = new UserAchievement
            {
                UserId = id,
                AchievementId = dto.AchievementId,
                UnlockedAt = DateTime.UtcNow
            };
            _context.UserAchievements.Add(userAchievement);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Achievement successfully added to user." });
        }

        // GET: api/admin/manage/users/{id}
        [HttpGet("users/{id}")]
        public async Task<ActionResult<UserAdminDetailsDto>> GetUserDetails(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound(new { message = "User not found." });

            var roles = await _userManager.GetRolesAsync(user);
            var userDetails = new UserAdminDetailsDto
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                DisplayName = user.DisplayName,
                RegistrationDate = user.RegistrationDate,
                Roles = roles,
                IsActive = user.IsActive
            };

            return Ok(userDetails);
        }

        // GET: api/admin/manage/users/{id}/achievements
        [HttpGet("users/{id}/achievements")]
        public async Task<ActionResult<IEnumerable<UserAchievementDto>>> GetUserAchievements(string id)
        {
            if (!await _userManager.Users.AnyAsync(u => u.Id == id)) return NotFound(new { message = "User not found." });

            var achievements = await _context.UserAchievements
                .Where(ua => ua.UserId == id)
                .Include(ua => ua.Achievement)
                .Select(ua => new UserAchievementDto
                {
                    Id = ua.Achievement.Id,
                    Name = ua.Achievement.Name,
                    Description = ua.Achievement.Description,
                    IconUrl = ua.Achievement.IconUrl,
                    UnlockedAt = ua.UnlockedAt
                })
                .ToListAsync();

            return Ok(achievements);
        }

        // --- Bundles Management ---
        [HttpGet("bundles")]
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

        [HttpPost("bundles")]
        public async Task<ActionResult<BundleDto>> CreateBundle([FromBody] CreateOrUpdateBundleDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var bundle = new Bundle
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                DiscountPercentage = dto.DiscountPercentage,
                DiscountPrice = dto.DiscountPercentage.HasValue && dto.DiscountPercentage.Value > 0
                    ? Math.Round(dto.Price * (1 - dto.DiscountPercentage.Value / 100m), 2)
                    : null,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                ImageUrl = dto.ImageUrl,
                IsActive = dto.IsActive
            };
            if (dto.GameIds != null)
            {
                foreach (var gameId in dto.GameIds)
                {
                    bundle.BundleGames.Add(new BundleGame { GameId = gameId });
                }
            }
            _context.Bundles.Add(bundle);
            await _context.SaveChangesAsync();
            var resultDto = new BundleDto
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
                GameIds = bundle.BundleGames.Select(bg => bg.GameId).ToList()
            };
            return CreatedAtAction(nameof(GetBundles), new { id = bundle.Id }, resultDto);
        }

        [HttpPut("bundles/{id}")]
        public async Task<IActionResult> UpdateBundle(int id, [FromBody] CreateOrUpdateBundleDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var bundle = await _context.Bundles.Include(b => b.BundleGames).FirstOrDefaultAsync(b => b.Id == id);
            if (bundle == null) return NotFound(new { message = "Bundle not found." });
            bundle.Name = dto.Name;
            bundle.Description = dto.Description;
            bundle.Price = dto.Price;
            bundle.DiscountPercentage = dto.DiscountPercentage;
            bundle.DiscountPrice = dto.DiscountPercentage.HasValue && dto.DiscountPercentage.Value > 0
                ? Math.Round(dto.Price * (1 - dto.DiscountPercentage.Value / 100m), 2)
                : null;
            bundle.StartDate = dto.StartDate;
            bundle.EndDate = dto.EndDate;
            bundle.ImageUrl = dto.ImageUrl;
            bundle.IsActive = dto.IsActive;
            _context.BundleGames.RemoveRange(bundle.BundleGames);
            if (dto.GameIds != null)
            {
                foreach (var gameId in dto.GameIds)
                {
                    bundle.BundleGames.Add(new BundleGame { GameId = gameId });
                }
            }
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Bundles.AnyAsync(b => b.Id == id)) return NotFound();
                throw;
            }
            return NoContent();
        }

        [HttpDelete("bundles/{id}")]
        public async Task<IActionResult> DeleteBundle(int id)
        {
            var bundle = await _context.Bundles.FindAsync(id);
            if (bundle == null) return NotFound(new { message = "Bundle not found." });
            _context.Bundles.Remove(bundle);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
} 