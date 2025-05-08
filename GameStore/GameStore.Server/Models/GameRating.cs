using System.ComponentModel.DataAnnotations;

namespace Games_Store.Models
{
    public class GameRating
    {
        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
        
        public int GameId { get; set; }
        public Game Game { get; set; } = null!;
        
        [Range(1, 10)]
        public int Score { get; set; }
        
        [StringLength(2000)]
        public string? Comment { get; set; }
        
        public DateTime RatingDate { get; set; } = DateTime.UtcNow;
        
        public DateTime? LastUpdated { get; set; }
    }
} 