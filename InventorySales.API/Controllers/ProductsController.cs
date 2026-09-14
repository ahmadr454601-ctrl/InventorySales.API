using InventorySales.API.Data;
using InventorySales.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventorySales.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts(
            string? search,
            int? categoryId)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            // Search by product name or SKU
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    p.Name.Contains(search) ||
                    p.SKU.Contains(search));
            }

            // Filter by category
            if (categoryId.HasValue)
            {
                query = query.Where(p =>
                    p.CategoryId == categoryId.Value);
            }

            return await query.ToListAsync();
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        // GET: api/Products/low-stock
        [HttpGet("low-stock")]
        public async Task<ActionResult<IEnumerable<Product>>> GetLowStockProducts()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Quantity <= p.LowStockLimit)
                .ToListAsync();

            return products;
        }

        // POST: api/Products
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            var categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == product.CategoryId);

            if (!categoryExists)
            {
                return BadRequest("Category does not exist.");
            }

            if (product.PurchasePrice <= 0)
            {
                return BadRequest("Purchase price must be greater than zero.");
            }

            if (product.SalePrice <= 0)
            {
                return BadRequest("Sale price must be greater than zero.");
            }

            if (product.Quantity < 0)
            {
                return BadRequest("Quantity cannot be negative.");
            }

            if (product.LowStockLimit < 0)
            {
                return BadRequest("Low stock limit cannot be negative.");
            }

            var skuExists = await _context.Products
                .AnyAsync(p => p.SKU == product.SKU);

            if (skuExists)
            {
                return BadRequest("SKU already exists.");
            }

            _context.Products.Add(product);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetProduct),
                new { id = product.Id },
                product);
        }

        // PUT: api/Products/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProduct(
            int id,
            Product product)
        {
            if (id != product.Id)
            {
                return BadRequest();
            }

            var categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == product.CategoryId);

            if (!categoryExists)
            {
                return BadRequest("Category does not exist.");
            }

            if (product.PurchasePrice <= 0)
            {
                return BadRequest("Purchase price must be greater than zero.");
            }

            if (product.SalePrice <= 0)
            {
                return BadRequest("Sale price must be greater than zero.");
            }

            if (product.Quantity < 0)
            {
                return BadRequest("Quantity cannot be negative.");
            }

            if (product.LowStockLimit < 0)
            {
                return BadRequest("Low stock limit cannot be negative.");
            }

            var skuExists = await _context.Products
                .AnyAsync(p => p.SKU == product.SKU && p.Id != id);

            if (skuExists)
            {
                return BadRequest("SKU already exists.");
            }

            var existingProduct = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);

            if (existingProduct == null)
            {
                return NotFound();
            }

            existingProduct.Name = product.Name;
            existingProduct.SKU = product.SKU;
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.PurchasePrice = product.PurchasePrice;
            existingProduct.SalePrice = product.SalePrice;
            existingProduct.Quantity = product.Quantity;
            existingProduct.Unit = product.Unit;
            existingProduct.LowStockLimit = product.LowStockLimit;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}