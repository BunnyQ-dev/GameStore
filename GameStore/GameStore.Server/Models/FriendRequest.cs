namespace Games_Store.Models
{
    public enum FriendRequestStatus
    {
        Pending,
        Accepted,
        Declined,
        Canceled
    }
    
    public class FriendRequest
    {
        public int Id { get; set; }
        
        public string SenderId { get; set; } = null!;
        public ApplicationUser Sender { get; set; } = null!;
        
        public string ReceiverId { get; set; } = null!;
        public ApplicationUser Receiver { get; set; } = null!;
        
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public FriendRequestStatus Status { get; set; } = FriendRequestStatus.Pending;
        
        public DateTime? ResponseDate { get; set; }
    }
} 