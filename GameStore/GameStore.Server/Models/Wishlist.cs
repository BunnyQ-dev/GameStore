namespace Games_Store.Models
{
    public class Wishlist
    {
        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
        
        public int GameId { get; set; }
        public Game Game { get; set; } = null!;
        
        public DateTime AddedDate { get; set; } = DateTime.UtcNow;
    }
} 