using System;
using System.Collections.Generic;

namespace Games_Store.DTOs
{
    /// <summary>
    /// DTO for creating or updating a bundle
    /// </summary>
    public class CreateOrUpdateBundleDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public List<int> GameIds { get; set; } = new List<int>();
    }

    /// <summary>
    /// DTO for returning bundle information
    /// </summary>
    public class BundleDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public decimal? DiscountPrice { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public List<int> GameIds { get; set; } = new List<int>();
        // Detailed info for games in this bundle
        public List<BundleGameDto> Games { get; set; } = new List<BundleGameDto>();
        // IDs of games the current user already owns (for discount calculation)
        public List<int> OwnedGameIds { get; set; } = new List<int>();
        // Total discount amount equal to the sum of owned game prices
        public decimal OwnedDiscount { get; set; }
        // Final price for the bundle after subtracting OwnedDiscount
        public decimal FinalPrice { get; set; }
    }

    /// <summary>
    /// DTO for game details in a bundle
    /// </summary>
    public class BundleGameDto
    {
        public int GameId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }
} 