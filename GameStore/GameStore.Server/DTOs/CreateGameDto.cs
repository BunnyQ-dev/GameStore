using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Games_Store.DTOs
{
    public class CreateGameDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        public DateTime? ReleaseDate { get; set; }

        [Range(0, 1000000)]
        public decimal Price { get; set; }

        [Range(0, 1000000)]
        public decimal? DiscountPrice { get; set; }

        public string? CoverImageUrl { get; set; }
        public string? TrailerUrl { get; set; }

        // IDs of related entities
        public List<int> GenreIds { get; set; } = new List<int>();
        public List<int> PlatformIds { get; set; } = new List<int>();
        public List<int> DeveloperIds { get; set; } = new List<int>();
        public List<int> PublisherIds { get; set; } = new List<int>();
        
        // public List<string> ScreenshotUrls { get; set; } = new List<string>();
    }
} 