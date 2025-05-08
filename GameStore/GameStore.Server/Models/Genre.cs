using System.ComponentModel.DataAnnotations;

namespace Games_Store.Models
{
    public class Genre
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = null!;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        // Navigation properties
        public ICollection<GameGenre> GameGenres { get; set; } = new List<GameGenre>();
    }
} 