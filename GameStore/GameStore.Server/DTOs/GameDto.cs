using System;
using System.Collections.Generic;

namespace Games_Store.DTOs
{
    public class GameDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime ReleaseDate { get; set; }
        public decimal Price { get; set; }
        public string? CoverImageUrl { get; set; }
        public string? TrailerUrl { get; set; }
        public bool IsActive { get; set; }
        public List<string> Genres { get; set; } = new();
        public List<string> Platforms { get; set; } = new();
        public List<string> Developers { get; set; } = new();
        public List<string> Publishers { get; set; } = new();
        public double Rating { get; set; }
        public List<int> DeveloperIds { get; set; } = new();
        public List<int> PublisherIds { get; set; } = new();
        public string? BackgroundImageUrl { get; set; }
        public string? MinimumSystemRequirements { get; set; }
        public string? RecommendedSystemRequirements { get; set; }
        public bool IsFeatured { get; set; }
        public int? Sales { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public List<string>? Screenshots { get; set; } = new();
        public string? ImageUrl { get; set; }
        public string? Developer { get; set; }
        public string? Publisher { get; set; }
        public decimal? OriginalPrice { get; set; }
        public double? AverageRating { get; set; }
        public int RatingsCount { get; set; }
        public bool IsOwned { get; set; }
        public bool IsInWishlist { get; set; }
        public int ReviewCount { get; set; }
    }
} 