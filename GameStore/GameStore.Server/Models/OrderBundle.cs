using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Games_Store.Models
{
    public class OrderBundle
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }
        [ForeignKey("OrderId")]
        public Order Order { get; set; } = null!;

        [Required]
        public int BundleId { get; set; }
        [ForeignKey("BundleId")]
        public Bundle Bundle { get; set; } = null!;

        [Required]
        public int Quantity { get; set; } = 1; 

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PriceAtPurchase { get; set; } 
    }
} 