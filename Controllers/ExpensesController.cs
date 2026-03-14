using HRMS.Data;
using HRMS.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HRMS.Controllers
{
    public class ExpensesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExpensesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // EMPLOYEE : My Expenses
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("Role") != "Employee")
                return RedirectToAction("Login", "Account");

            int? employeeId = HttpContext.Session.GetInt32("EmployeeId");
            if (employeeId == null)
                return RedirectToAction("Login", "Account");

            var expenses = await _context.Expenses
                .Where(e => e.EmployeeId == employeeId)
                .OrderByDescending(e => e.Date)
                .ToListAsync();

            return View(expenses);
        }

        // HR : All Employee Expense Requests
        public async Task<IActionResult> HRList()
        {
            if (HttpContext.Session.GetString("Role") != "HR")
                return RedirectToAction("Login", "Account");

            var list = await _context.Expenses
                .Include(e => e.Employee)
                .OrderByDescending(e => e.Date)
                .ToListAsync();

            return View(list);
        }

        // EMPLOYEE : Create Expense
        [HttpGet]
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("Role") != "Employee")
                return RedirectToAction("Login", "Account");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Expenses expense, IFormFile? ProofFile)
        {
            int? employeeId = HttpContext.Session.GetInt32("EmployeeId");
            if (employeeId == null)
                return RedirectToAction("Login", "Account");

            // Validate date
            if (expense.Date == DateTime.MinValue || expense.Date.Year < 2000)
            {
                TempData["Error"] = "Please select a valid date!";
                return View(expense);
            }

            // Validate model
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();

                TempData["Error"] = "Model Errors: " + string.Join(" | ", errors);
                return View(expense);
            }

            // Assign employee & status
            expense.EmployeeId = employeeId.Value;
            expense.Status = "Pending";

            // Upload file if available
            expense.ProofFilePath = await UploadProof(ProofFile);

            // Save
            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Expense saved successfully!";
            return RedirectToAction(nameof(Index));
        }


        // EDIT
        public async Task<IActionResult> Edit(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Employee")
                return RedirectToAction("Login", "Account");

            int? employeeId = HttpContext.Session.GetInt32("EmployeeId");
            if (employeeId == null)
                return RedirectToAction("Login", "Account");

            var expense = await _context.Expenses.FindAsync(id);

            if (expense == null || expense.EmployeeId != employeeId)
                return RedirectToAction("Index");

            if (expense.Status != "Pending")
                return RedirectToAction("Index");

            return View(expense);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Expenses expense, IFormFile? ProofFile)
        {
            if (id != expense.Id)
                return RedirectToAction("Index");

            int? employeeId = HttpContext.Session.GetInt32("EmployeeId");
            if (employeeId == null)
                return RedirectToAction("Login", "Account");

            var dbExpense = await _context.Expenses.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);

            if (dbExpense == null || dbExpense.EmployeeId != employeeId)
                return RedirectToAction("Index");

            if (!ModelState.IsValid)
                return View(expense);

            if (ProofFile != null)
                expense.ProofFilePath = await UploadProof(ProofFile);
            else
                expense.ProofFilePath = dbExpense.ProofFilePath;

            expense.EmployeeId = employeeId.Value;
            expense.Status = dbExpense.Status;

            _context.Update(expense);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // DELETE
        public async Task<IActionResult> Delete(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Employee")
                return RedirectToAction("Login", "Account");

            int? employeeId = HttpContext.Session.GetInt32("EmployeeId");
            if (employeeId == null)
                return RedirectToAction("Login", "Account");

            var expense = await _context.Expenses.FindAsync(id);

            if (expense == null || expense.EmployeeId != employeeId)
                return RedirectToAction("Index");

            return View(expense);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            int? employeeId = HttpContext.Session.GetInt32("EmployeeId");
            if (employeeId == null)
                return RedirectToAction("Login", "Account");

            var expense = await _context.Expenses.FindAsync(id);

            if (expense != null && expense.EmployeeId == employeeId)
            {
                if (!string.IsNullOrEmpty(expense.ProofFilePath))
                {
                    string fullPath = Path.Combine("wwwroot", expense.ProofFilePath.TrimStart('/'));
                    if (System.IO.File.Exists(fullPath))
                        System.IO.File.Delete(fullPath);
                }

                _context.Expenses.Remove(expense);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // HR Approve
        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            if (HttpContext.Session.GetString("Role") != "HR")
                return RedirectToAction("Login", "Account");

            var exp = await _context.Expenses.FindAsync(id);
            if (exp == null)
                return RedirectToAction(nameof(HRList));

            exp.Status = "Approved";
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(HRList));
        }

        // HR Reject
        [HttpPost]
        public async Task<IActionResult> Reject(int id, string? comment)
        {
            if (HttpContext.Session.GetString("Role") != "HR")
                return RedirectToAction("Login", "Account");

            var exp = await _context.Expenses.FindAsync(id);
            if (exp == null)
                return RedirectToAction(nameof(HRList));

            exp.Status = "Rejected";
            exp.HRComment = comment;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(HRList));
        }

        // FILE UPLOAD
        private async Task<string?> UploadProof(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return null;

            string folder = Path.Combine("wwwroot", "uploads", "proofs");
            Directory.CreateDirectory(folder);

            string filename = Guid.NewGuid() + Path.GetExtension(file.FileName);
            string fullpath = Path.Combine(folder, filename);

            using var stream = new FileStream(fullpath, FileMode.Create);
            await file.CopyToAsync(stream);

            return "/uploads/proofs/" + filename;
        }
    }
}
