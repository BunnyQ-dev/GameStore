using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Games_Store.Models
{
    // Update the `Order` class to include the `OrderItems` property
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public ICollection<OrderBundle> Bundles { get; set; } = new List<OrderBundle>();
    }
} 