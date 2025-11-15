using HRMS.Data;
using HRMS.Models;
using HRMS.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // ✅ 1️⃣ Default HR login (hardcoded)
            const string defaultHrId = "HR001";
            const string defaultHrPassword = "admin123";

            if (model.UserId.Equals(defaultHrId, StringComparison.OrdinalIgnoreCase)
                && model.Password == defaultHrPassword)
            {
                HttpContext.Session.SetString("Role", "HR");
                HttpContext.Session.SetString("HrName", "Admin HR");
                return RedirectToAction("Index", "Home"); // HR dashboard
            }

            // ✅ 2️⃣ Check Employee from Database
            var emp = _context.Employees
                .FirstOrDefault(e => e.EmployeeCode == model.UserId && e.Password == model.Password);

            if (emp != null)
            {
                HttpContext.Session.SetString("Role", "Employee");
                HttpContext.Session.SetInt32("EmployeeId", emp.Id);
                HttpContext.Session.SetString("EmployeeName", emp.Name);
               
                HttpContext.Session.SetString("Role", emp.Role);
                return RedirectToAction("Dashboard", "Employees");
            }
            else

                // ❌ Invalid credentials
                ViewBag.Error = "Invalid ID or Password.";
            return View(model);

           
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
