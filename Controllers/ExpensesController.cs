using HRMS.Data;
using HRMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace HRMS.Controllers
{
    public class ExpensesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExpensesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Expenses
        public async Task<IActionResult> Index()
        {
            var expenses = await _context.Expenses.ToListAsync();

            ViewBag.Summary = expenses
                .GroupBy(e => e.ExpenseType)
                .Select(g => new { Type = g.Key, Total = g.Sum(e => e.Amount) })
                .ToList();

            return View(expenses);
        }

        // GET: /Expenses/Create
        public IActionResult Create()
        {
            return View();
        }

        // ✅ POST: /Expenses/Create (With File Upload)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Expenses expense, IFormFile? ProofFile)
        {
            if (ModelState.IsValid)
            {
                // ✅ Handle File Upload
                if (ProofFile != null && ProofFile.Length > 0)
                {
                    // Set upload directory
                    string uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "proofs");

                    if (!Directory.Exists(uploadDir))
                        Directory.CreateDirectory(uploadDir);

                    // Create unique filename
                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(ProofFile.FileName);
                    string filePath = Path.Combine(uploadDir, uniqueFileName);

                    // Save file
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ProofFile.CopyToAsync(fileStream);
                    }

                    // Store relative path in DB
                    expense.ProofFilePath = "/uploads/proofs/" + uniqueFileName;
                }

                _context.Add(expense);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(expense);
        }

        // GET: /Expenses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null)
                return NotFound();

            return View(expense);
        }

        // ✅ POST: /Expenses/Edit/5 (Supports updating proof file)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Expenses expense, IFormFile? ProofFile)
        {
            if (id != expense.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Handle updated file upload
                    if (ProofFile != null && ProofFile.Length > 0)
                    {
                        string uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "proofs");

                        if (!Directory.Exists(uploadDir))
                            Directory.CreateDirectory(uploadDir);

                        string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(ProofFile.FileName);
                        string filePath = Path.Combine(uploadDir, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await ProofFile.CopyToAsync(fileStream);
                        }

                        // Update file path
                        expense.ProofFilePath = "/uploads/proofs/" + uniqueFileName;
                    }

                    _context.Update(expense);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Expenses.Any(e => e.Id == expense.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(expense);
        }

        // GET: /Expenses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null)
                return NotFound();

            return View(expense);
        }

        // POST: /Expenses/Delete/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense != null)
            {
                // Optional: delete proof file from folder
                if (!string.IsNullOrEmpty(expense.ProofFilePath))
                {
                    string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", expense.ProofFilePath.TrimStart('/'));
                    if (System.IO.File.Exists(fullPath))
                        System.IO.File.Delete(fullPath);
                }

                _context.Expenses.Remove(expense);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: /Expenses/Approve
        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense != null)
            {
                expense.Status = "Approved";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: /Expenses/Reject
        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense != null)
            {
                expense.Status = "Rejected";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
