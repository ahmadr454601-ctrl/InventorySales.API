using InventorySales.API.Data;
using InventorySales.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventorySales.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SalesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Sales
        [HttpGet]
        public async Task<IActionResult> GetSales()
        {
            var sales = await _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.Items)
                    .ThenInclude(i => i.Product)
                .Select(s => new
                {
                    s.Id,
                    s.CustomerId,

                    Customer = s.Customer == null
                        ? null
                        : new
                        {
                            s.Customer.Id,
                            s.Customer.Name,
                            s.Customer.Phone,
                            s.Customer.Email,
                            s.Customer.Address
                        },

                    s.SaleDate,
                    s.TotalAmount,

                    Items = s.Items.Select(i => new
                    {
                        i.Id,
                        i.SaleId,
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
                        i.SalePrice,
                        i.TotalPrice
                    })
                })
                .ToListAsync();

            return Ok(sales);
        }

        // GET: api/Sales/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSale(int id)
        {
            var sale = await _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sale == null)
            {
                return NotFound();
            }

            var result = new
            {
                sale.Id,
                sale.CustomerId,

                Customer = sale.Customer == null
                    ? null
                    : new
                    {
                        sale.Customer.Id,
                        sale.Customer.Name,
                        sale.Customer.Phone,
                        sale.Customer.Email,
                        sale.Customer.Address
                    },

                sale.SaleDate,
                sale.TotalAmount,

                Items = sale.Items.Select(i => new
                {
                    i.Id,
                    i.SaleId,
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
                    i.SalePrice,
                    i.TotalPrice
                })
            };

            return Ok(result);
        }

        // POST: api/Sales
        [HttpPost]
        public async Task<IActionResult> CreateSale(Sale sale)
        {
            var customerExists = await _context.Customers
                .AnyAsync(c => c.Id == sale.CustomerId);

            if (!customerExists)
            {
                return BadRequest("Customer does not exist.");
            }

            if (sale.Items == null || sale.Items.Count == 0)
            {
                return BadRequest("At least one product is required.");
            }

            using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var item in sale.Items)
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

                    if (item.SalePrice <= 0)
                    {
                        return BadRequest(
                            "Sale price must be greater than zero.");
                    }

                    if (product.Quantity < item.Quantity)
                    {
                        return BadRequest(
                            $"Not enough stock for product '{product.Name}'. Available stock: {product.Quantity}.");
                    }

                    // Calculate item total
                    item.TotalPrice =
                        item.Quantity * item.SalePrice;

                    // Decrease product stock
                    product.Quantity -= item.Quantity;
                }

                // Calculate total sale amount
                sale.TotalAmount =
                    sale.Items.Sum(i => i.TotalPrice);

                // Add sale and its items
                _context.Sales.Add(sale);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return CreatedAtAction(
                    nameof(GetSale),
                    new { id = sale.Id },
                    new
                    {
                        sale.Id,
                        sale.CustomerId,
                        sale.SaleDate,
                        sale.TotalAmount,

                        Items = sale.Items.Select(i => new
                        {
                            i.Id,
                            i.SaleId,
                            i.ProductId,
                            i.Quantity,
                            i.SalePrice,
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
                        innerException =
                            ex.InnerException?.Message
                    });
            }
        }

        // DELETE: api/Sales/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSale(int id)
        {
            var sale = await _context.Sales
                .Include(s => s.Items)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sale == null)
            {
                return NotFound();
            }

            // Restore stock
            foreach (var item in sale.Items)
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(
                        p => p.Id == item.ProductId);

                if (product != null)
                {
                    product.Quantity += item.Quantity;
                }
            }

            // Delete sale and its items
            _context.Sales.Remove(sale);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}