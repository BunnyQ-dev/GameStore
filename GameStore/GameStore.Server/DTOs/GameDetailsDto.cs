using System;
using System.Collections.Generic;

namespace Games_Store.DTOs
{
    // DTO for Screenshot
    public class ScreenshotDto
    {
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty;
    }

    // DTO for Rating/Review
    public class RatingDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Review { get; set; }
        public DateTime DatePosted { get; set; }
    }

    public class GameDetailsDto : GameDto
    {
        public string? BackgroundUrl { get; set; }
        public string? MinimumRequirements { get; set; }
        public string? RecommendedRequirements { get; set; }
        public new List<string> Platforms { get; set; } = new List<string>();
        public List<ScreenshotDto> Screenshots { get; set; } = new List<ScreenshotDto>();
        public new decimal Rating { get; set; }
        public int RatingsCount { get; set; }
        public bool IsInWishlist { get; set; }
        public bool IsPurchased { get; set; }
        public List<RatingDto> Ratings { get; set; } = new List<RatingDto>();
        public double? AverageRating { get; set; }
        public bool IsOwned { get; set; }
        public int ReviewCount { get; set; }
    }
} 