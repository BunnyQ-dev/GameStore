namespace Games_Store.Models
{
    public class BundleGame
    {
        public int BundleId { get; set; }
        public Bundle Bundle { get; set; } = null!;
        
        public int GameId { get; set; }
        public Game Game { get; set; } = null!;
    }
} 