using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Games_Store.DTOs
{
    public class AdminGameDetailsDto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 2)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Range(0, 10000)]
        public decimal Price { get; set; }

        [Range(0, 100)]
        public decimal? DiscountPercentage { get; set; }
        
        public decimal? DiscountPrice { get; set; }

        public DateTime? ReleaseDate { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; } = false;

        // For select dropdowns (single selection)
        public int? DeveloperId { get; set; }
        public int? PublisherId { get; set; }

        // For multi-select (IDs for updating)
        public List<int> GenreIds { get; set; } = new List<int>();
        public List<int> PlatformIds { get; set; } = new List<int>();

        // For displaying names/details (full DTOs)
        public List<GenreDto> Genres { get; set; } = new List<GenreDto>();
        public List<PlatformDto> Platforms { get; set; } = new List<PlatformDto>();
        public List<DeveloperDto> Developers { get; set; } = new List<DeveloperDto>(); 
        public List<PublisherDto> Publishers { get; set; } = new List<PublisherDto>();


        public string? CoverImageUrl { get; set; }
        public string? BackgroundImageUrl { get; set; }
        public string? TrailerUrl { get; set; }

        public string? MinimumSystemRequirements { get; set; }
        public string? RecommendedSystemRequirements { get; set; }

        public List<string> ScreenshotUrls { get; set; } = new List<string>(); 
        

    }
} 