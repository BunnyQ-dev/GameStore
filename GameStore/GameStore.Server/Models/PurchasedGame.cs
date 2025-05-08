namespace Games_Store.Models
{
    public class PurchasedGame
    {
        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
        
        public int GameId { get; set; }
        public Game Game { get; set; } = null!;
        
        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
        
        public decimal PricePaid { get; set; }
    }
} 