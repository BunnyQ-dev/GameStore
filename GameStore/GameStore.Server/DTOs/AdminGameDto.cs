using System;
using System.Collections.Generic;

namespace Games_Store.DTOs
{
    public class AdminGameDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }
        public decimal DiscountPercentage { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string CoverImageUrl { get; set; } = null!;
        public string BackgroundImageUrl { get; set; } = null!;
        public string TrailerUrl { get; set; } = null!;
        public string MinimumSystemRequirements { get; set; } = null!;
        public string RecommendedSystemRequirements { get; set; } = null!;
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }
        public List<int> GenreIds { get; set; } = new List<int>();
        public List<int> PlatformIds { get; set; } = new List<int>();
        public List<int> DeveloperIds { get; set; } = new List<int>();
        public List<int> PublisherIds { get; set; } = new List<int>();
        public List<string> Screenshots { get; set; } = new List<string>();
    }
} 