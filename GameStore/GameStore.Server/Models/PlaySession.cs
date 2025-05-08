namespace Games_Store.Models
{
    public class PlaySession
    {
        public int Id { get; set; }
        
        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
        
        public int GameId { get; set; }
        public Game Game { get; set; } = null!;
        
        public DateTime StartTime { get; set; }
        
        public DateTime? EndTime { get; set; }
        
        public decimal HoursPlayed { get; set; }
        
        public string? DeviceInfo { get; set; }
    }
} 