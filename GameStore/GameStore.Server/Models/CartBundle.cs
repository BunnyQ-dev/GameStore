using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Games_Store.Models
{
    public class CartBundle
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CartId { get; set; }
        [ForeignKey("CartId")]
        public Cart Cart { get; set; } = null!;

        [Required]
        public int BundleId { get; set; }
        [ForeignKey("BundleId")]
        public Bundle Bundle { get; set; } = null!;

        [Required]
        public int Quantity { get; set; } = 1;

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }
    }
} 