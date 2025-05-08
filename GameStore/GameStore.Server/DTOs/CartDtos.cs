namespace Games_Store.DTOs
{
    // Single definition of CartItemDto
    public class CartItemDto
    {
        public int Id { get; set; }
        public int GameId { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; } // Price at the time of addition, or current game price?
        public decimal? DiscountPercentage { get; set; }
        public decimal? OriginalPrice { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int Quantity { get; set; } // Typically 1 for games
    }

    // DTO for bundles in the cart
    public class CartBundleDto
    {
        public int Id { get; set; }
        public int BundleId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        // List of game IDs in this bundle for checkout
        public List<int> gameIds { get; set; } = new List<int>();
    }

    // Single definition of CartDto with unambiguous Items property usage
    public class CartDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
        public List<CartBundleDto> Bundles { get; set; } = new List<CartBundleDto>();
        public decimal TotalPrice => 
            Items.Sum(item => item.Price * item.Quantity)
          + Bundles.Sum(b => b.Price * b.Quantity);
        public decimal TotalOriginalPrice => Items
            .Where(i => i.OriginalPrice.HasValue)
            .Sum(item => item.OriginalPrice!.Value * item.Quantity);
        public decimal TotalDiscount => TotalOriginalPrice - TotalPrice;
    }
}
