using Games_Store.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Games_Store.Services;

namespace Games_Store.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IUserEventHandler _userEventHandler;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            IUserEventHandler userEventHandler)
        {
            _userManager = userManager;
            _configuration = configuration;
            _userEventHandler = userEventHandler;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
                DisplayName = string.IsNullOrEmpty(model.DisplayName) ? model.Username : model.DisplayName,
                RegistrationDate = DateTime.UtcNow
            };

            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return StatusCode(StatusCodes.Status409Conflict, new { Status = "Error", Message = "User already exists!" });

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { Status = "Error", Message = "User creation failed! Please check user details and try again.", Errors = result.Errors });

            await _userEventHandler.HandleUserCreatedAsync(user);

            return Ok(new { Status = "Success", Message = "User created successfully!" });
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                return Unauthorized(new { Status = "Error", Message = "Invalid username or password" });

            // Update last login date
            user.LastLoginDate = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            var userRoles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var token = GetToken(authClaims);

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo,
                user = new
                {
                    user.Id,
                    user.UserName,
                    user.DisplayName,
                    user.Email,
                    user.ProfilePictureUrl,
                    Roles = userRoles
                }
            });
        }

        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
            var tokenExpiryMinutes = int.Parse(_configuration["JWT:TokenExpiryMinutes"] ?? "60");

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddMinutes(tokenExpiryMinutes),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return token;
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me()
        {
            var userName = User.Identity?.Name;
            if (userName == null) return Unauthorized();

            var user = await _userManager.FindByNameAsync(userName);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            var userProfile = new UserProfileDto
            {
                Id = user.Id,
                Username = user.UserName,
                DisplayName = user.DisplayName,
                Email = user.Email,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Roles = roles
            };

            return Ok(userProfile);
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileModel model)
        {
            var userName = User.Identity?.Name;
            if (userName == null) return Unauthorized();

            var user = await _userManager.FindByNameAsync(userName);
            if (user == null) return NotFound();

            user.DisplayName = model.DisplayName ?? user.DisplayName;
            user.Email = model.Email ?? user.Email;

            await _userManager.UpdateAsync(user);

            return Ok(new { Status = "Success" });
        }

        public class UpdateProfileModel
        {
            public string? DisplayName { get; set; }
            public string? Email { get; set; }
        }

        [HttpGet("public-users")]
        [AllowAnonymous]
        public IActionResult GetPublicUsers([FromQuery] string? query = null)
        {
            var usersQuery = _userManager.Users.AsQueryable();
            if (!string.IsNullOrWhiteSpace(query))
            {
                usersQuery = usersQuery.Where(u => u.UserName.Contains(query) || u.DisplayName.Contains(query));
            }
            var users = usersQuery.Select(u => new
            {
                u.Id,
                u.UserName,
                u.DisplayName
            }).ToList();
            return Ok(users);
        }

        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetUsers()
        {
            var users = _userManager.Users.Select(u => new
            {
                u.Id,
                u.UserName,
                u.Email,
                u.DisplayName
            }).ToList();
            return Ok(users);
        }

        [HttpGet("public-user/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublicUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var isSelf = User.Identity?.IsAuthenticated == true && user.UserName == User.Identity.Name;

            return Ok(new
            {
                user.Id,
                user.UserName,
                user.DisplayName,
                user.ProfilePictureUrl,
                Email = isSelf ? user.Email : null,
                CanEdit = isSelf
            });
        }

        [HttpPost("avatar")]
        [Authorize]
        public async Task<IActionResult> UploadAvatar([FromForm] IFormFile avatar)
        {
            if (avatar == null || avatar.Length == 0)
                return BadRequest("No file uploaded");

            var userName = User.Identity?.Name;
            if (userName == null) return Unauthorized();

            var user = await _userManager.FindByNameAsync(userName);
            if (user == null) return NotFound();

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "avatars");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileExt = Path.GetExtension(avatar.FileName);
            var fileName = $"{user.Id}_{Guid.NewGuid()}{fileExt}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await avatar.CopyToAsync(stream);
            }

            user.ProfilePictureUrl = $"/avatars/{fileName}";
            await _userManager.UpdateAsync(user);

            return Ok(new { url = user.ProfilePictureUrl });
        }
    }

    public class RegisterModel
    {
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? DisplayName { get; set; }
    }

    public class LoginModel
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class UserProfileDto
    {
        public string Id { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string? DisplayName { get; set; }
        public string Email { get; set; } = null!;
        public string? ProfilePictureUrl { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
    }
} 