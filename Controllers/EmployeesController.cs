using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRMS.Data;
using HRMS.Models;
using HRMS.Models.ViewModels;

namespace HRMS.Controllers
{
    [Route("[controller]/[action]")]
    [Route("Employee/[action]")]  // allows both /Employees/... and /Employee/...
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // INDEX - Display all employees (for HR)
        // ============================================================
        [HttpGet]
        public IActionResult Index()
        {
            // Only HR can view full employee list
            var role = HttpContext.Session.GetString("Role");
            if (role != "HR")
                return RedirectToAction("Login", "Account");

            var employees = _context.Employees.ToList();
            return View(employees);
        }

        // ============================================================
        // DASHBOARD - Employee Personal Dashboard
        // ============================================================
        [HttpGet]
        public IActionResult Dashboard()
        {
            var empId = HttpContext.Session.GetInt32("EmployeeId");
            if (empId == null)
                return RedirectToAction("Login", "Account");

            var emp = _context.Employees.FirstOrDefault(e => e.Id == empId);
            if (emp == null)
                return RedirectToAction("Login", "Account");

            // Example: count total leaves if you have a Leave table
            var totalLeaves = _context.Leaves.Count(l => l.EmployeeId == emp.Id);
            var approvedLeaves = _context.Leaves.Count(l => l.EmployeeId == emp.Id && l.Status == "Approved");

            var dashboard = new EmployeeDashboardViewModel
            {
                EmployeeName = emp.Name,
                Department = emp.Department,
                Position = emp.Position,
                TotalLeaves = totalLeaves,
                ApprovedLeaves = approvedLeaves
            };

            return View(dashboard);
        }

        // ============================================================
        // CREATE EMPLOYEE (HR only)
        // ============================================================
        [HttpGet]
        public IActionResult Create()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "HR")
                return RedirectToAction("Login", "Account");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Employee model)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "HR")
                return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                _context.Employees.Add(model);
                _context.SaveChanges();
                TempData["Success"] = "Employee added successfully!";
                return RedirectToAction("Index");
            }

            return View(model);
        }

        // ============================================================
        // EDIT EMPLOYEE (HR only)
        // ============================================================
        [HttpGet("{id}")]
        public IActionResult Edit(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "HR")
                return RedirectToAction("Login", "Account");

            var employee = _context.Employees.Find(id);
            if (employee == null)
                return NotFound();

            return View(employee);
        }

        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Employee model)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "HR")
                return RedirectToAction("Login", "Account");

            if (id != model.Id)
                return BadRequest();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    _context.SaveChanges();
                    TempData["Success"] = "Employee details updated successfully!";
                    return RedirectToAction("Index");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Employees.Any(e => e.Id == model.Id))
                        return NotFound();
                    else
                        throw;
                }
            }

            return View(model);
        }
    }
}
