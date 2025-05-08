namespace Games_Store.DTOs
{
    public class OrderBundleDto
    {
        public int BundleId { get; set; }
        public string BundleName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal PriceAtPurchase { get; set; }
    }
} 