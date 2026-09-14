using InventorySales.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventorySales.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReportsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Reports/Summary
        [HttpGet("Summary")]
        public async Task<IActionResult> GetSummary()
        {
            var totalSales = await _context.Sales
                .SumAsync(s => (decimal?)s.TotalAmount) ?? 0;

            var totalPurchases = await _context.Purchases
                .SumAsync(p => (decimal?)p.TotalAmount) ?? 0;

            var totalExpenses = await _context.Expenses
                .SumAsync(e => (decimal?)e.Amount) ?? 0;

            var totalProducts = await _context.Products
                .CountAsync();

            var totalCustomers = await _context.Customers
                .CountAsync();

            var totalSuppliers = await _context.Suppliers
                .CountAsync();

            var profitLoss =
                totalSales - totalPurchases - totalExpenses;

            return Ok(new
            {
                totalSales,
                totalPurchases,
                totalExpenses,
                totalProducts,
                totalCustomers,
                totalSuppliers,
                profitLoss
            });
        }

        // GET: api/Reports/Sales
        [HttpGet("Sales")]
        public async Task<IActionResult> GetSalesReport()
        {
            var sales = await _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.Items)
                    .ThenInclude(i => i.Product)
                .OrderByDescending(s => s.SaleDate)
                .Select(s => new
                {
                    s.Id,
                    Customer = s.Customer == null
                        ? "Unknown"
                        : s.Customer.Name,
                    s.SaleDate,
                    s.TotalAmount,
                    Items = s.Items.Count
                })
                .ToListAsync();

            return Ok(sales);
        }

        // GET: api/Reports/Purchases
        [HttpGet("Purchases")]
        public async Task<IActionResult> GetPurchasesReport()
        {
            var purchases = await _context.Purchases
                .Include(p => p.Supplier)
                .Include(p => p.Items)
                .OrderByDescending(p => p.PurchaseDate)
                .Select(p => new
                {
                    p.Id,
                    Supplier = p.Supplier == null
                        ? "Unknown"
                        : p.Supplier.Name,
                    p.PurchaseDate,
                    p.TotalAmount,
                    Items = p.Items.Count
                })
                .ToListAsync();

            return Ok(purchases);
        }

        // GET: api/Reports/Expenses
        [HttpGet("Expenses")]
        public async Task<IActionResult> GetExpensesReport()
        {
            var expenses = await _context.Expenses
                .OrderByDescending(e => e.ExpenseDate)
                .Select(e => new
                {
                    e.Id,
                    e.Title,
                    e.Amount,
                    e.Description,
                    e.ExpenseDate
                })
                .ToListAsync();

            return Ok(expenses);
        }
    }
}