using System.ComponentModel.DataAnnotations;

namespace Games_Store.Models
{
    public class Publisher
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        [StringLength(200)]
        public string? Website { get; set; }
        
        public string? LogoUrl { get; set; }
        
        public DateTime? FoundedDate { get; set; }
        
        // Navigation properties
        public ICollection<GamePublisher> GamePublishers { get; set; } = new List<GamePublisher>();
        
    }
} 