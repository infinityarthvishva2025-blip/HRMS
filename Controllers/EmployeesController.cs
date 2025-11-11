using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRMS.Data;
using HRMS.Models;
using HRMS.Models.ViewModels;

namespace HRMS.Controllers
{
    [Route("[controller]/[action]")]
    [Route("Employee/[action]")]
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public EmployeesController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ============================================================
        // INDEX - HR Employee List
        // ============================================================
        [HttpGet]
        public IActionResult Index()
        {
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
        // CREATE EMPLOYEE
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
        public IActionResult Create(Employee model,
            IFormFile? AadharCard,
            IFormFile? PanCard,
            IFormFile? Marksheet,
            IFormFile? ProfilePhoto,
            IFormFile? BankPassbook)
        {
            if (ModelState.IsValid)
            {
                string uploadPath = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                model.AadharCardPath = AadharCard != null ? SaveFile(AadharCard, uploadPath) : null;
                model.PanCardPath = PanCard != null ? SaveFile(PanCard, uploadPath) : null;
                model.MarksheetPath = Marksheet != null ? SaveFile(Marksheet, uploadPath) : null;
                model.ProfilePhotoPath = ProfilePhoto != null ? SaveFile(ProfilePhoto, uploadPath) : null;
                model.BankPassbookPath = BankPassbook != null ? SaveFile(BankPassbook, uploadPath) : null;

                _context.Employees.Add(model);
                _context.SaveChanges();

                TempData["Success"] = "Employee added successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // ============================================================
        // EDIT EMPLOYEE
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
        public IActionResult Edit(int id, Employee model,
            IFormFile? AadharCard,
            IFormFile? PanCard,
            IFormFile? Marksheet,
            IFormFile? ProfilePhoto,
            IFormFile? BankPassbook)
        {
            if (id != model.Id)
                return BadRequest();

            if (ModelState.IsValid)
            {
                var existing = _context.Employees.AsNoTracking().FirstOrDefault(e => e.Id == id);
                if (existing == null)
                    return NotFound();

                string uploadPath = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                model.AadharCardPath = AadharCard != null ? SaveFile(AadharCard, uploadPath) : existing.AadharCardPath;
                model.PanCardPath = PanCard != null ? SaveFile(PanCard, uploadPath) : existing.PanCardPath;
                model.MarksheetPath = Marksheet != null ? SaveFile(Marksheet, uploadPath) : existing.MarksheetPath;
                model.ProfilePhotoPath = ProfilePhoto != null ? SaveFile(ProfilePhoto, uploadPath) : existing.ProfilePhotoPath;
                model.BankPassbookPath = BankPassbook != null ? SaveFile(BankPassbook, uploadPath) : existing.BankPassbookPath;

                _context.Update(model);
                _context.SaveChanges();

                TempData["Success"] = "Employee updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // ============================================================
        // DELETE EMPLOYEE
        // ============================================================
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var employee = _context.Employees.Find(id);
            if (employee == null)
                return NotFound();

            _context.Employees.Remove(employee);
            _context.SaveChanges();

            TempData["Success"] = "Employee deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // HELPER METHOD - FILE SAVE
        // ============================================================
        private string SaveFile(IFormFile file, string uploadPath)
        {
            string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(uploadPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }
            return "/uploads/" + fileName;
        }
    }
}
