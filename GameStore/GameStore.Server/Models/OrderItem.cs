using System.ComponentModel.DataAnnotations;

namespace Games_Store.Models
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;
        public int GameId { get; set; }
        public Game Game { get; set; } = null!;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
} 