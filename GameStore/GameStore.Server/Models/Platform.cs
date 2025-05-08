using System.ComponentModel.DataAnnotations;

namespace Games_Store.Models
{
    public class Platform
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = null!;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public string? LogoUrl { get; set; }
        
        // Navigation properties
        public ICollection<GamePlatform> GamePlatforms { get; set; } = new List<GamePlatform>();
    }
} 