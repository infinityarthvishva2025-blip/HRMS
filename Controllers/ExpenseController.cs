using HRMS.Data;
using HRMS.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace HRMS.Controllers
{
    public class ExpenseController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExpenseController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var expenses = _context.Expenses.ToList();

            // Group by expense type for pie chart
            var summary = expenses
                .GroupBy(e => e.ExpenseType)
                .Select(g => new
                {
                    Type = g.Key,
                    Total = g.Sum(e => e.Amount)
                }).ToList();

            ViewBag.Summary = summary;
            return View(expenses);
        }

        [HttpPost]
        public IActionResult Create(Expense expense)
        {
            if (ModelState.IsValid)
            {
                _context.Expenses.Add(expense);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View("Index");
        }

        public IActionResult Approve(int id)
        {
            var expense = _context.Expenses.Find(id);
            if (expense != null)
            {
                expense.Status = "Approved";
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public IActionResult Reject(int id)
        {
            var expense = _context.Expenses.Find(id);
            if (expense != null)
            {
                expense.Status = "Rejected";
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
