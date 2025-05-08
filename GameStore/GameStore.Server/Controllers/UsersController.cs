using Games_Store.Data;
using Games_Store.Models;
using Games_Store.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting; // Для IWebHostEnvironment
using System.IO; // Для Path
using System.Linq;
using System.Collections.Generic;

namespace Games_Store.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UsersController(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager, 
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // Removed the incorrect usage of 'modelBuilder' in the GetCurrentUser method.
        // The 'modelBuilder' is used in the context of Entity Framework's OnModelCreating method, not in a controller.

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var isAdmin = roles.Contains("Admin");

            var purchasedGamesCount = await _context.PurchasedGames.CountAsync(pg => pg.UserId == userId);
            var friendsCount = await _context.Friends

                 .CountAsync(f => f.UserId == userId || f.FriendId == userId); 

            var wishlistCount = await _context.Wishlists
                .CountAsync(w => w.UserId == userId);

            var friendsPreview = await _context.Friends
                .Where(f => f.UserId == userId || f.FriendId == userId)
                .OrderByDescending(f => f.FriendshipDate)
                .Take(6)
                .Select(f => new {
                    friendUser = f.UserId == userId ? f.FriendUser : f.User
                })
                .Select(f => new FriendPreviewDto {
                    UserId = f.friendUser.Id,
                    UserName = f.friendUser.UserName,
                    DisplayName = f.friendUser.DisplayName,
                    ProfilePictureUrl = f.friendUser.ProfilePictureUrl
                })
                .ToListAsync();

            var profileDto = new ProfileDetailDto
            {
                UserId = user.Id,
                UserName = user.UserName,
                DisplayName = user.DisplayName,
                Email = user.Email,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Bio = user.Bio,
                RegistrationDate = user.RegistrationDate,
                FriendsCount = friendsCount,
                WishlistCount = wishlistCount,
                GamesOwned = purchasedGamesCount,
                FriendshipStatus = "self",
                FriendsPreview = friendsPreview,
                Roles = roles
            };

            return Ok(profileDto);
        }

        // GET: api/users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var isAdmin = roles.Contains("Admin");
            
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isCurrentUser = currentUserId == id;

            var purchasedGamesCount = await _context.PurchasedGames
                .Where(pg => pg.UserId == id)
                .CountAsync();

            var friendsCount = await _context.Friends
                .Where(f => f.UserId == id || f.FriendId == id)
                .CountAsync();

            var reviewsCount = await _context.GameRatings
                .Where(gr => gr.UserId == id)
                .CountAsync();

            // Визначення статусу дружби з поточним користувачем
            string friendshipStatus = "none";
            List<FriendPreviewDto> friendsPreview = new List<FriendPreviewDto>();

            if (!string.IsNullOrEmpty(currentUserId) && currentUserId != id)
            {
                var areFriends = await _context.Friends
                    .AnyAsync(f => (f.UserId == currentUserId && f.FriendId == id) || 
                                   (f.UserId == id && f.FriendId == currentUserId));
                
                if (areFriends)
                {
                    friendshipStatus = "accepted";
                }
                else
                {
                    var pendingRequestSent = await _context.FriendRequests
                        .AnyAsync(fr => fr.SenderId == currentUserId && fr.ReceiverId == id);
                    
                    var pendingRequestReceived = await _context.FriendRequests
                        .AnyAsync(fr => fr.SenderId == id && fr.ReceiverId == currentUserId);

                    if (pendingRequestSent)
                    {
                        friendshipStatus = "pending_sent";
                    }
                    else if (pendingRequestReceived)
                    {
                        friendshipStatus = "pending_received";
                    }
                }
            }

            // Отримуємо друзів для прев'ю
            friendsPreview = await _context.Friends
                .Where(f => f.UserId == id || f.FriendId == id)
                .OrderByDescending(f => f.FriendshipDate)
                .Take(6)
                .Select(f => new {
                    friendUser = f.UserId == id ? f.FriendUser : f.User
                })
                .Select(f => new FriendPreviewDto {
                    UserId = f.friendUser.Id,
                    UserName = f.friendUser.UserName,
                    DisplayName = f.friendUser.DisplayName,
                    ProfilePictureUrl = f.friendUser.ProfilePictureUrl
                })
                .ToListAsync();

            return Ok(new
            {
                id = user.Id,
                username = user.UserName,
                email = user.Email,
                displayName = user.DisplayName,
                profilePictureUrl = user.ProfilePictureUrl,
                bio = user.Bio,
                createdAt = user.RegistrationDate,
                lastLoginDate = user.LastLoginDate,
                isActive = user.IsActive,
                isAdmin,
                isCurrentUser,
                gamesCount = purchasedGamesCount,
                friendsCount = friendsCount,
                reviewsCount = reviewsCount,
                friendshipStatus,
                friendsPreview = friendsPreview
            });
        }

        // GET: api/users/{id}/achievements
        [HttpGet("{id}/achievements")]
        [AllowAnonymous] // Allow public access to achievements
        public async Task<ActionResult<IEnumerable<UserAchievementDto>>> GetUserAchievements(string id)
        {
            if (!await _userManager.Users.AnyAsync(u => u.Id == id))
                return NotFound(new { message = "Користувача не знайдено." });

            var achievements = await _context.UserAchievements
                .Where(ua => ua.UserId == id)
                .Include(ua => ua.Achievement)
                .OrderByDescending(ua => ua.UnlockedAt) // Optionally order by unlock date
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

        // PUT: api/users/me
        [HttpPut("me")]
        [Authorize]
        public async Task<IActionResult> UpdateCurrentUser(
            [FromForm] string DisplayName, 
            [FromForm] string? Bio, 
            IFormFile? AvatarFile)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();
            
            var webRootPath = _webHostEnvironment.WebRootPath;
            if (string.IsNullOrEmpty(webRootPath))
            {
                Console.Error.WriteLine("IWebHostEnvironment.WebRootPath is null or empty. Check wwwroot configuration and static file serving.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Server configuration error for file operations." });
            }

            user.DisplayName = DisplayName;
            user.Bio = Bio;

            string? savedRelativePath = null;

            if (AvatarFile != null && AvatarFile.Length > 0)
            {
                if (!AvatarFile.ContentType.StartsWith("image/"))
                {
                     return BadRequest(new { message = "Unsupported file type for avatar." });
                }

                if (!string.IsNullOrEmpty(user.ProfilePictureUrl) && user.ProfilePictureUrl != "/img/default-avatar.jpg")
                {

                    var oldFilePath = Path.Combine(webRootPath, user.ProfilePictureUrl.TrimStart('/')); 
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        try { System.IO.File.Delete(oldFilePath); }
                        catch (IOException ex) { /* Log deletion error */ }
                    }
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(AvatarFile.FileName);
                var uploadsFolder = Path.Combine(webRootPath, "uploads", "avatars"); 
                var filePath = Path.Combine(uploadsFolder, fileName);
                savedRelativePath = "/uploads/avatars/" + fileName; 
                
                Directory.CreateDirectory(uploadsFolder);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await AvatarFile.CopyToAsync(stream);
                }

                user.ProfilePictureUrl = savedRelativePath; 
            }
            
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {

                if (savedRelativePath != null)
                {
                     var filePathToDelete = Path.Combine(webRootPath, savedRelativePath.TrimStart('/'));
                     if (System.IO.File.Exists(filePathToDelete))
                     {
                          try { System.IO.File.Delete(filePathToDelete); }
                          catch (IOException ex) { /* Log deletion error */ }
                     }
                }
                return BadRequest(new { message = "Profile update error", errors = result.Errors.Select(e => e.Description) });
            }


             return Ok(new {
                id = user.Id,
                username = user.UserName,
                email = user.Email,
                displayName = user.DisplayName,
                profilePictureUrl = user.ProfilePictureUrl,
                bio = user.Bio,
                registrationDate = user.RegistrationDate,
             });
        }

        // POST: api/users/change-password
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new { message = "Password changed successfully" });
        }
        
        // GET: api/users/profile/{userId}
        [HttpGet("profile/{userId}")]
        [AllowAnonymous] // Дозволити анонімний доступ до профілів
        public async Task<ActionResult<ProfileDetailDto>> GetUserProfile(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var purchasedGamesCount = await _context.PurchasedGames
                .CountAsync(pg => pg.UserId == userId);
            
            var wishlistCount = await _context.Wishlists
                .CountAsync(w => w.UserId == userId);
            
            var friendsCount = await _context.Friends
                 .CountAsync(f => f.UserId == userId || f.FriendId == userId);

            string friendshipStatus = "none";
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier); 

            if (!string.IsNullOrEmpty(currentUserId))
            {
                if (currentUserId == userId)
                {
                    friendshipStatus = "self";
                }
                else
                {
                    var areFriends = await _context.Friends
                        .AnyAsync(f => (f.UserId == currentUserId && f.FriendId == userId) || 
                                       (f.UserId == userId && f.FriendId == currentUserId));
                    
                    if (areFriends)
                    {
                        friendshipStatus = "accepted";
                    }
                    else
                    {
                        var pendingRequest = await _context.FriendRequests
                            .FirstOrDefaultAsync(fr => 
                                ((fr.SenderId == currentUserId && fr.ReceiverId == userId) || 
                                 (fr.SenderId == userId && fr.ReceiverId == currentUserId)) &&
                                fr.Status == FriendRequestStatus.Pending);
                                
                        if (pendingRequest != null)
                        {
                            if (pendingRequest.SenderId == currentUserId)
                            {
                                friendshipStatus = "pending_sent"; 
                            }
                            else
                            {
                                friendshipStatus = "pending_received"; 
                            }
                        }
                    }
                }
            } else if (currentUserId == userId) {
                friendshipStatus = "self"; 
            }
                
            var friendsPreview = await _context.Friends
                .Where(f => f.UserId == userId || f.FriendId == userId)
                .OrderByDescending(f => f.FriendshipDate)
                .Take(6)
                .Select(f => f.UserId == userId ? f.FriendUser : f.User)
                .Select(fUser => new FriendPreviewDto
                {
                    UserId = fUser.Id,
                    UserName = fUser.UserName,
                    DisplayName = fUser.DisplayName,
                    ProfilePictureUrl = fUser.ProfilePictureUrl
                }).ToListAsync();

            var profileDto = new ProfileDetailDto
            {
                UserId = user.Id,
                UserName = user.UserName,
                DisplayName = user.DisplayName,
                Email = currentUserId == userId ? user.Email : null,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Bio = user.Bio,
                RegistrationDate = user.RegistrationDate,
                FriendsCount = friendsCount,
                WishlistCount = wishlistCount,
                GamesOwned = purchasedGamesCount,
                FriendshipStatus = friendshipStatus,
                FriendsPreview = friendsPreview
            };
            
            return Ok(profileDto);
        }
    }

    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }

    public class ProfileDetailDto
    {
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string? DisplayName { get; set; }
        public string? Email { get; set; } 
        public string? ProfilePictureUrl { get; set; }
        public string? Bio { get; set; }
        public DateTime RegistrationDate { get; set; }
        public int FriendsCount { get; set; }
        public int WishlistCount { get; set; }
        public int GamesOwned { get; set; } 
        public string? FriendshipStatus { get; set; } 
        public List<FriendPreviewDto> FriendsPreview { get; set; } = new List<FriendPreviewDto>(); 
        public IList<string>? Roles { get; set; } 
    }

    public class FriendPreviewDto 
    {
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string? DisplayName { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public int FriendsCount { get; set; }
    }
} 