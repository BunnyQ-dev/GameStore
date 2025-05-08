using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Games_Store.Models
{
    // Join table for User <-> Achievement relationship (Many-to-Many)
    public class UserAchievement
    {
        [Required]
        public string UserId { get; set; } = null!; // Foreign key to ApplicationUser
        public ApplicationUser User { get; set; } = null!; // Navigation property

        [Required]
        public int AchievementId { get; set; } // Foreign key to Achievement
        public Achievement Achievement { get; set; } = null!; // Navigation property

        [Required]
        public DateTime UnlockedAt { get; set; } = DateTime.UtcNow; 

    }
} 