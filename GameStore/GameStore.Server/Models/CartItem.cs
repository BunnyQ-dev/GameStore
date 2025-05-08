using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Games_Store.Models
{
    public class CartItem
    {
        public CartItem()
        {
        }
        
        [Key]
        public int Id { get; set; }

        [Required]
        public int GameId { get; set; }
        
        [ForeignKey("GameId")]
        public Game Game { get; set; } = null!;

        [Required]
        public int CartId { get; set; }
        
        [ForeignKey("CartId")]
        public Cart Cart { get; set; } = null!;

        [Required]
        public int Quantity { get; set; }
        
        public decimal Price { get; set; }
    }
} 