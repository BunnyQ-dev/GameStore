using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Games_Store.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            Wishlists = new List<Wishlist>();
            PurchasedGames = new List<PurchasedGame>();
            FriendsInitiated = new List<Friend>();
            FriendsReceived = new List<Friend>();
            FriendRequestsSent = new List<FriendRequest>();
            FriendRequestsReceived = new List<FriendRequest>();
            GameRatings = new List<GameRating>();
            UserAchievements = new List<UserAchievement>();
            PlaySessions = new List<PlaySession>();
        }

        [StringLength(100)]
        public string? DisplayName { get; set; }
        
        public string? ProfilePictureUrl { get; set; }
        
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
        
        public DateTime? LastLoginDate { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        [StringLength(1000)]
        public string? Bio { get; set; }
        
        // Navigation properties for user relationships
        public ICollection<Wishlist> Wishlists { get; set; }
        
        public ICollection<PurchasedGame> PurchasedGames { get; set; }
        
        // Friends relationships
        public ICollection<Friend> FriendsInitiated { get; set; }
        public ICollection<Friend> FriendsReceived { get; set; }
        
        public ICollection<FriendRequest> FriendRequestsSent { get; set; }
        public ICollection<FriendRequest> FriendRequestsReceived { get; set; }
        
        public ICollection<GameRating> GameRatings { get; set; }
        
        public ICollection<UserAchievement> UserAchievements { get; set; }
        
        public ICollection<PlaySession> PlaySessions { get; set; }
    }
} 