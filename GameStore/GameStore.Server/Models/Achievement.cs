using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Games_Store.Models
{
    public class Achievement
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int GameId { get; set; } // Foreign key to Game
        public Game Game { get; set; } = null!; // Navigation property

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = null!;

        public string? IconUrl { get; set; }

        public int Points { get; set; } = 10; 

        public bool IsSecret { get; set; } 

        public ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
    }
} 