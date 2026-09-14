namespace InventorySales.API.Models
{
    public class ShopSetting
    {
        public int Id { get; set; }

        public string ShopName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Logo { get; set; }

        public string? InvoiceFooter { get; set; }
    }
}