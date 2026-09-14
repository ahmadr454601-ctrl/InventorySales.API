namespace InventorySales.API.Models
{
    public class Sale
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public Customer? Customer { get; set; }

        public DateTime SaleDate { get; set; } = DateTime.Now;

        public decimal TotalAmount { get; set; }

        public List<SaleItem> Items { get; set; } = new();
    }
}