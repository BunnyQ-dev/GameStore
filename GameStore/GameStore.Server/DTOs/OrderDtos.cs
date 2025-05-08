namespace Games_Store.DTOs
{
    public class OrderItemCreateDto
    {
        public int GameId { get; set; }
        public int Quantity { get; set; } = 1; 
    }

    public class OrderItemDto
    {
        public int GameId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; } 
        public string ImageUrl { get; set; } = string.Empty;
    }

    public class OrderDto
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
    }
} 