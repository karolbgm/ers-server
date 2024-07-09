using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ers_server.Data;
using ers_server.Models;
using Humanizer;
using System.Buffers.Text;

namespace ers_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpensesController : ControllerBase
    {
        private readonly ErsDbContext _context;

        public ExpensesController(ErsDbContext context)
        {
            _context = context;
        }

        //PayExpense(expenseId) : [PUT: /api/expenses/pay/{expenseId}] 
        [HttpPut("pay/{expenseId}")]

        public async Task<IActionResult> PayExpense(int expenseId)
        {
            var foundExpense = await _context.Expenses.FindAsync(expenseId);
            var foundEmployee = await _context.Employees.FindAsync(foundExpense!.EmployeeId);

            foundExpense.Status = "PAID";
            foundEmployee!.ExpensesPaid = foundExpense.Total;
            foundEmployee.ExpensesDue -= foundExpense.Total;
            await _context.SaveChangesAsync();

            return Ok();


        }

        //UpdateEmployeeExpensesDueAndPaid(employeeId)
        
        private async Task<IActionResult> UpdateEmployeeExpensesDueAndPaid(int employeeId)
        {
            var foundEmployee = await _context.Employees.FindAsync(employeeId);

            var totalDue = (from emp in _context.Employees
                               join exp in _context.Expenses
                               on emp.Id equals exp.EmployeeId
                               where (emp.Id == employeeId && exp.Status == "DUE")
                               select new
                               {
                                   Total = exp.Total
                               }).Sum(x => x.Total); 
            
            var totalPaid = (from emp in _context.Employees
                               join exp in _context.Expenses
                               on emp.Id equals exp.EmployeeId
                               where (emp.Id == employeeId && exp.Status == "PAID")
                               select new
                               {
                                   Total = exp.Total
                               }).Sum(x => x.Total);

            foundEmployee!.ExpensesPaid = totalPaid;
            foundEmployee.ExpensesDue = totalDue;
            await _context.SaveChangesAsync();

            return Ok();

        }

        //GetApprovedExpenses() : [GET: /api/expenses/approved] - Retrieves all expenses with a status of APPROVED
        [HttpGet("approved")]
        public async Task<ActionResult<IEnumerable<Expense>>> GetApprovedExpenses()
        {
            return await _context.Expenses.Where(x => x.Status == "APPROVED").ToListAsync();
        }

        //GetExpensesInReview() : [GET: /api/expenses/review] - Retrieves all expenses with a status of REVIEW
        [HttpGet("review")]
        public async Task<ActionResult<IEnumerable<Expense>>> GetExpensesReview()
        {
            return await _context.Expenses.Where(x => x.Status == "REVIEW").ToListAsync();
        }

        //ReviewExpense(id, expense) : [PUT: /api/expenses/review/{id}] - Sets the Status property based on the Total property

        [HttpPut("review/{id}")]

        public async Task<IActionResult> ReviewExpense(int id, Expense expense)
        {
            var foundExpense = await _context.Expenses.FindAsync(id);
            var foundEmployee = await _context.Employees.FindAsync(foundExpense!.EmployeeId);

            if (foundExpense.Total <= 75)
            {
                foundExpense.Status = "APPROVED";
                foundEmployee!.ExpensesDue += foundExpense.Total;
                await _context.SaveChangesAsync();


            } else
            {
                foundExpense.Status = "REVIEW";
                await _context.SaveChangesAsync(); //ASK GREG-------------------------------
            }
           
            return Ok();

        }
        //ApproveExpense(id, expense) : [PUT: /api/expenses/approve/{id}] -Unconditionally sets the Status property to APPROVED and add the Total to the Employee ExpensesDue
        [HttpPut("approve/{id}")]

        public async Task<IActionResult> ApproveExpense(int id, Expense expense)
        {
            var foundExpense = await _context.Expenses.FindAsync(id);
            var foundEmployee = await _context.Employees.FindAsync(foundExpense!.EmployeeId);

            foundExpense.Status = "APPROVED";
            foundEmployee!.ExpensesDue += foundExpense.Total;
            await _context.SaveChangesAsync();
            
             return Ok();
        }

        //RejectExpense(id, expense) : [PUT: /api/expenses/reject/{id}] -Unconditionally sets the Status property to REJECTED. No change is needed in the Employee ExpensesDue or ExpensesPaid.
        [HttpPut("reject/{id}")]

        public async Task<IActionResult> RejectExpense(int id, Expense expense)
        {
            var foundExpense = await _context.Expenses.FindAsync(id);
            var foundEmployee = await _context.Employees.FindAsync(foundExpense!.EmployeeId);

            foundExpense.Status = "REJECTED";
            await _context.SaveChangesAsync();

            return Ok();
        }

        // GET: api/Expenses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Expense>>> GetExpenses()
        {
            return await _context.Expenses.ToListAsync();
        }

        // GET: api/Expenses/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Expense>> GetExpense(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);

            if (expense == null)
            {
                return NotFound();
            }

            return expense;
        }

        // PUT: api/Expenses/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutExpense(int id, Expense expense)
        {
            if (id != expense.Id)
            {
                return BadRequest();
            }

            _context.Entry(expense).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ExpenseExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Expenses
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Expense>> PostExpense(Expense expense)
        {
            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetExpense", new { id = expense.Id }, expense);
        }

        // DELETE: api/Expenses/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExpense(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null)
            {
                return NotFound();
            }

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ExpenseExists(int id)
        {
            return _context.Expenses.Any(e => e.Id == id);
        }
    }
}
