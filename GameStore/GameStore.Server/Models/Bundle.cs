using System.ComponentModel.DataAnnotations;

namespace Games_Store.Models
{
    public class Bundle
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        [Range(0, 10000)]
        public decimal Price { get; set; }
        
        public decimal? DiscountPercentage { get; set; }
        
        public decimal? DiscountPrice { get; set; }
        
        public DateTime? StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }
        
        public string? ImageUrl { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public ICollection<BundleGame> BundleGames { get; set; } = new List<BundleGame>();
        public ICollection<CartBundle> CartBundles { get; set; } = new List<CartBundle>();
    }
} 