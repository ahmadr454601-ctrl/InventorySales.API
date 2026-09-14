namespace InventorySales.API.Models
{
    public class Purchase
    {
        public int Id { get; set; }

        public int SupplierId { get; set; }

        public Supplier? Supplier { get; set; }

        public DateTime PurchaseDate { get; set; } = DateTime.Now;

        public decimal TotalAmount { get; set; }

        public List<PurchaseItem> Items { get; set; } = new();
    }
}