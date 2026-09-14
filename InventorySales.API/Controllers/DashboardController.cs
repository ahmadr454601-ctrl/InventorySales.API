using InventorySales.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventorySales.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Dashboard
        [HttpGet]
        public async Task<IActionResult> GetDashboard()
        {
            // =========================
            // BASIC COUNTS
            // =========================

            var totalProducts =
                await _context.Products.CountAsync();

            var totalCustomers =
                await _context.Customers.CountAsync();

            var totalSuppliers =
                await _context.Suppliers.CountAsync();

            var totalCategories =
                await _context.Categories.CountAsync();


            // =========================
            // FINANCIAL TOTALS
            // =========================

            var totalSales =
                await _context.Sales
                    .SumAsync(s => (decimal?)s.TotalAmount) ?? 0;

            var totalPurchases =
                await _context.Purchases
                    .SumAsync(p => (decimal?)p.TotalAmount) ?? 0;

            var totalExpenses =
                await _context.Expenses
                    .SumAsync(e => (decimal?)e.Amount) ?? 0;


            // =========================
            // COST OF GOODS SOLD
            // =========================

            var totalCostOfGoodsSold =
                await _context.SaleItems
                    .Include(i => i.Product)
                    .SumAsync(i =>
                        (decimal?)(
                            i.Quantity *
                            i.Product.PurchasePrice
                        )
                    ) ?? 0;


            // =========================
            // STOCK
            // =========================

            var totalStock =
                await _context.Products
                    .SumAsync(p => (int?)p.Quantity) ?? 0;

            var lowStockProducts =
                await _context.Products
                    .Where(p =>
                        p.Quantity <= p.LowStockLimit)
                    .CountAsync();


            // =========================
            // PROPER PROFIT / LOSS
            // =========================

            var grossProfit =
                totalSales -
                totalCostOfGoodsSold;

            var estimatedProfit =
                grossProfit -
                totalExpenses;


            // =========================
            // RECENT SALES
            // =========================

            var recentSales =
                await _context.Sales
                    .Include(s => s.Customer)
                    .OrderByDescending(s => s.SaleDate)
                    .Take(5)
                    .Select(s => new
                    {
                        s.Id,

                        Customer =
                            s.Customer == null
                                ? "Unknown"
                                : s.Customer.Name,

                        s.SaleDate,
                        s.TotalAmount
                    })
                    .ToListAsync();


            // =========================
            // RECENT PURCHASES
            // =========================

            var recentPurchases =
                await _context.Purchases
                    .Include(p => p.Supplier)
                    .OrderByDescending(p => p.PurchaseDate)
                    .Take(5)
                    .Select(p => new
                    {
                        p.Id,

                        Supplier =
                            p.Supplier == null
                                ? "Unknown"
                                : p.Supplier.Name,

                        p.PurchaseDate,
                        p.TotalAmount
                    })
                    .ToListAsync();


            // =========================
            // LOW STOCK PRODUCTS
            // =========================

            var lowStockList =
                await _context.Products
                    .Where(p =>
                        p.Quantity <= p.LowStockLimit)
                    .OrderBy(p => p.Quantity)
                    .Take(5)
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.SKU,
                        p.Quantity,
                        p.LowStockLimit,
                        p.Unit
                    })
                    .ToListAsync();


            // =========================
            // FINAL RESPONSE
            // =========================

            return Ok(new
            {
                TotalProducts = totalProducts,
                TotalCustomers = totalCustomers,
                TotalSuppliers = totalSuppliers,
                TotalCategories = totalCategories,

                TotalSales = totalSales,
                TotalPurchases = totalPurchases,
                TotalExpenses = totalExpenses,

                TotalCostOfGoodsSold = totalCostOfGoodsSold,

                TotalStock = totalStock,
                LowStockProducts = lowStockProducts,

                GrossProfit = grossProfit,
                EstimatedProfit = estimatedProfit,

                RecentSales = recentSales,
                RecentPurchases = recentPurchases,
                LowStockList = lowStockList
            });
        }
    }
}