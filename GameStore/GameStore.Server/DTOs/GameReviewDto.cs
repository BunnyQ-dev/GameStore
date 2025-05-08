namespace Games_Store.DTOs
{
    public class GameReviewDto
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? UserDisplayName { get; set; }
        public string? UserAvatarUrl { get; set; }
        public int GameId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsCurrentUserReview { get; set; } 
    }
} 