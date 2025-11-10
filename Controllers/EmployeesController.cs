using HRMS.Data;
using HRMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var employees = await _context.Employees.AsNoTracking().ToListAsync();
            return View(employees);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee employee)
        {
            if (ModelState.IsValid)
            {
                // Generate unique Employee Code automatically
                var lastEmployee = await _context.Employees
                    .OrderByDescending(e => e.EmployeeCode)
                    .FirstOrDefaultAsync();

                int nextNumber = 1;
                if (lastEmployee != null && !string.IsNullOrEmpty(lastEmployee.EmployeeCode))
                {
                    // Extract numeric part (e.g., from EMP005 → 5)
                    string numericPart = new string(lastEmployee.EmployeeCode
                        .Where(char.IsDigit)
                        .ToArray());

                    if (int.TryParse(numericPart, out int lastNumber))
                        nextNumber = lastNumber + 1;
                }

                // Format as EMP001, EMP002, etc.
                employee.EmployeeCode = $"EMP{nextNumber:D3}";

                // Default employee status
                employee.Status = "Active";

                _context.Add(employee);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Employee {employee.EmployeeCode} added successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(employee);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();
            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Employee employee)
        {
            if (id != employee.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(employee);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Employee updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(employee);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var emp = await _context.Employees.FindAsync(id);
            if (emp == null) return NotFound();

            _context.Employees.Remove(emp);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Employee deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
