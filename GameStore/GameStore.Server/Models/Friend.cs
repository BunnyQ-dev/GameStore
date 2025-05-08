namespace Games_Store.Models
{
    public class Friend
    {
        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
        
        public string FriendId { get; set; } = null!;
        public ApplicationUser FriendUser { get; set; } = null!;
        
        public DateTime FriendshipDate { get; set; } = DateTime.UtcNow;
    }
} 