using InventorySales.API.Data;
using InventorySales.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventorySales.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchasesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PurchasesController(AppDbContext context)
        {
            _context = context;
        }

        // =========================
        // GET ALL PURCHASES
        // =========================

        [HttpGet]
        public async Task<IActionResult> GetPurchases()
        {
            var purchases = await _context.Purchases
                .Include(p => p.Supplier)
                .Include(p => p.Items)
                    .ThenInclude(i => i.Product)
                .Select(p => new
                {
                    p.Id,
                    p.SupplierId,

                    Supplier = p.Supplier == null
                        ? null
                        : new
                        {
                            p.Supplier.Id,
                            p.Supplier.Name,
                            p.Supplier.Phone,
                            p.Supplier.Email,
                            p.Supplier.Address
                        },

                    p.PurchaseDate,
                    p.TotalAmount,

                    Items = p.Items.Select(i => new
                    {
                        i.Id,
                        i.PurchaseId,
                        i.ProductId,

                        Product = i.Product == null
                            ? null
                            : new
                            {
                                i.Product.Id,
                                i.Product.Name,
                                i.Product.SKU
                            },

                        i.Quantity,
                        i.PurchasePrice,
                        i.TotalPrice
                    })
                })
                .ToListAsync();

            return Ok(purchases);
        }

        // =========================
        // GET PURCHASE BY ID
        // =========================

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPurchase(int id)
        {
            var purchase = await _context.Purchases
                .Include(p => p.Supplier)
                .Include(p => p.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (purchase == null)
            {
                return NotFound();
            }

            var result = new
            {
                purchase.Id,
                purchase.SupplierId,

                Supplier = purchase.Supplier == null
                    ? null
                    : new
                    {
                        purchase.Supplier.Id,
                        purchase.Supplier.Name,
                        purchase.Supplier.Phone,
                        purchase.Supplier.Email,
                        purchase.Supplier.Address
                    },

                purchase.PurchaseDate,
                purchase.TotalAmount,

                Items = purchase.Items.Select(i => new
                {
                    i.Id,
                    i.PurchaseId,
                    i.ProductId,

                    Product = i.Product == null
                        ? null
                        : new
                        {
                            i.Product.Id,
                            i.Product.Name,
                            i.Product.SKU
                        },

                    i.Quantity,
                    i.PurchasePrice,
                    i.TotalPrice
                })
            };

            return Ok(result);
        }

        // =========================
        // CREATE PURCHASE
        // =========================

        [HttpPost]
        public async Task<IActionResult> CreatePurchase(Purchase purchase)
        {
            var supplierExists = await _context.Suppliers
                .AnyAsync(s => s.Id == purchase.SupplierId);

            if (!supplierExists)
            {
                return BadRequest("Supplier does not exist.");
            }

            if (purchase.Items == null || purchase.Items.Count == 0)
            {
                return BadRequest("At least one product is required.");
            }

            using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var item in purchase.Items)
                {
                    var product = await _context.Products
                        .FirstOrDefaultAsync(
                            p => p.Id == item.ProductId);

                    if (product == null)
                    {
                        return BadRequest(
                            $"Product with ID {item.ProductId} does not exist.");
                    }

                    if (item.Quantity <= 0)
                    {
                        return BadRequest(
                            "Quantity must be greater than zero.");
                    }

                    if (item.PurchasePrice <= 0)
                    {
                        return BadRequest(
                            "Purchase price must be greater than zero.");
                    }

                    item.TotalPrice =
                        item.Quantity * item.PurchasePrice;

                    // Increase stock
                    product.Quantity += item.Quantity;
                }

                // Calculate total
                purchase.TotalAmount =
                    purchase.Items.Sum(i => i.TotalPrice);

                // Save Purchase and PurchaseItems together
                _context.Purchases.Add(purchase);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return CreatedAtAction(
                    nameof(GetPurchase),
                    new
                    {
                        id = purchase.Id
                    },
                    new
                    {
                        purchase.Id,
                        purchase.SupplierId,
                        purchase.PurchaseDate,
                        purchase.TotalAmount,

                        Items = purchase.Items.Select(i => new
                        {
                            i.Id,
                            i.PurchaseId,
                            i.ProductId,
                            i.Quantity,
                            i.PurchasePrice,
                            i.TotalPrice
                        })
                    });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return StatusCode(
                    500,
                    new
                    {
                        message = ex.Message,
                        innerException = ex.InnerException?.Message
                    });
            }
        }

        // =========================
        // DELETE PURCHASE
        // =========================

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePurchase(int id)
        {
            var purchase = await _context.Purchases
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (purchase == null)
            {
                return NotFound();
            }

            foreach (var item in purchase.Items)
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(
                        p => p.Id == item.ProductId);

                if (product != null)
                {
                    // Decrease stock
                    product.Quantity -= item.Quantity;

                    if (product.Quantity < 0)
                    {
                        product.Quantity = 0;
                    }
                }
            }

            _context.Purchases.Remove(purchase);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}