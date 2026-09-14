namespace InventorySales.API.Models
{
    public class Product
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string SKU { get; set; } = string.Empty;

        public int CategoryId { get; set; }

        public Category? Category { get; set; }

        public decimal PurchasePrice { get; set; }

        public decimal SalePrice { get; set; }

        public int Quantity { get; set; }

        public string Unit { get; set; } = string.Empty;

        public int LowStockLimit { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}