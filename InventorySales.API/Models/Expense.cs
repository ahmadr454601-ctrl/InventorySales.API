namespace InventorySales.API.Models
{
    public class Expense
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string? Description { get; set; }

        public DateTime ExpenseDate { get; set; } = DateTime.Now;
    }
}