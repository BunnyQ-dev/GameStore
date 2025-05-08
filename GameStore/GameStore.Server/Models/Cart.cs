using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Games_Store.Models
{
    public class Cart
    {
        public Cart()
        {
            Items = new List<CartItem>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = null!;
        
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } = null!;

        public ICollection<CartItem> Items { get; set; }

        // Bundles in the cart
        public ICollection<CartBundle> CartBundles { get; set; } = new List<CartBundle>();
    }
} 