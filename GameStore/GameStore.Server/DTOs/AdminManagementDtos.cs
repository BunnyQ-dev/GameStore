namespace Games_Store.DTOs
{
    public class CreateOrUpdateGenreDto
    {
        public string Name { get; set; } = null!;
    }

    public class CreateOrUpdateDeveloperDto
    {
        public string Name { get; set; } = null!;
    }

    public class CreateOrUpdatePublisherDto
    {
        public string Name { get; set; } = null!;
    }

    public class CreateOrUpdateAchievementDto
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int GameId { get; set; }
        public string IconUrl { get; set; } = null!; 
    }

    public class AchievementDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string IconUrl { get; set; } = null!;
        public int GameId { get; set; } 
    }

    public class UserAdminDto
    {
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string? Email { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
        public bool IsActive { get; set; }
    }

    public class UserAdminDetailsDto
    {
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string? Email { get; set; }
        public string? DisplayName { get; set; }
        public DateTime RegistrationDate { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
        public bool IsActive { get; set; }
    }

    public class UserAchievementDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string IconUrl { get; set; } = string.Empty;
        public DateTime UnlockedAt { get; set; }
    }

    public class UserAdminUpdateDto
    {
        public IList<string> Roles { get; set; } = new List<string>();
        public bool IsActive { get; set; }
    }

    public class CreateOrUpdatePlatformDto
    {
        public string Name { get; set; } = null!;
    }

    public class PlatformDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }
} 