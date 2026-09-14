using InventorySales.API.Data;
using InventorySales.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventorySales.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpensesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ExpensesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Expenses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Expense>>> GetExpenses()
        {
            return await _context.Expenses
                .OrderByDescending(e => e.ExpenseDate)
                .ToListAsync();
        }

        // GET: api/Expenses/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Expense>> GetExpense(int id)
        {
            var expense = await _context.Expenses
                .FirstOrDefaultAsync(e => e.Id == id);

            if (expense == null)
            {
                return NotFound();
            }

            return expense;
        }

        // POST: api/Expenses
        [HttpPost]
        public async Task<ActionResult<Expense>> CreateExpense(Expense expense)
        {
            if (string.IsNullOrWhiteSpace(expense.Title))
            {
                return BadRequest("Expense title is required.");
            }

            if (expense.Amount <= 0)
            {
                return BadRequest("Expense amount must be greater than zero.");
            }

            if (expense.ExpenseDate == default)
            {
                expense.ExpenseDate = DateTime.Now;
            }

            _context.Expenses.Add(expense);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetExpense),
                new { id = expense.Id },
                expense);
        }

        // PUT: api/Expenses/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateExpense(
            int id,
            Expense expense)
        {
            if (id != expense.Id)
            {
                return BadRequest();
            }

            if (string.IsNullOrWhiteSpace(expense.Title))
            {
                return BadRequest("Expense title is required.");
            }

            if (expense.Amount <= 0)
            {
                return BadRequest("Expense amount must be greater than zero.");
            }

            var existingExpense = await _context.Expenses
                .FirstOrDefaultAsync(e => e.Id == id);

            if (existingExpense == null)
            {
                return NotFound();
            }

            existingExpense.Title = expense.Title;
            existingExpense.Amount = expense.Amount;
            existingExpense.Description = expense.Description;
            existingExpense.ExpenseDate = expense.ExpenseDate;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Expenses/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExpense(int id)
        {
            var expense = await _context.Expenses
                .FirstOrDefaultAsync(e => e.Id == id);

            if (expense == null)
            {
                return NotFound();
            }

            _context.Expenses.Remove(expense);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}