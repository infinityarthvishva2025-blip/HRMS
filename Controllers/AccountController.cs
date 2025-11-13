using HRMS.Data;
using HRMS.Models;
using HRMS.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace HRMS.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // -----------------------------------------------
        // PASSWORD HASHING HELPER
        // -----------------------------------------------
        private static string HashPassword(string password)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        // -----------------------------------------------
        // LOGIN (GET)
        // -----------------------------------------------
        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        // -----------------------------------------------
        // LOGIN (POST)
        // -----------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // 1️⃣ DEFAULT HR LOGIN
            const string defaultHrId = "HR001";
            const string defaultHrPassword = "admin123";

            if (model.UserId.Equals(defaultHrId, StringComparison.OrdinalIgnoreCase)
                && model.Password == defaultHrPassword)
            {
                HttpContext.Session.SetString("Role", "HR");
                HttpContext.Session.SetString("HrName", "Admin HR");

                return RedirectToAction("Index", "Home");
            }

            // 2️⃣ EMPLOYEE LOGIN
            string hashedPassword = HashPassword(model.Password);

            var emp = _context.Employees
                .FirstOrDefault(e => e.EmployeeCode == model.UserId
                                  && e.Password == hashedPassword);

            if (emp != null)
            {
                HttpContext.Session.SetString("Role", "Employee");
                HttpContext.Session.SetInt32("EmployeeId", emp.Id);
                HttpContext.Session.SetString("EmployeeName", emp.Name);

                return RedirectToAction("Dashboard", "Employees");
            }

            // ❌ INVALID LOGIN
            ViewBag.Error = "Invalid ID or Password.";
            return View(model);
        }

        // -----------------------------------------------
        // LOGOUT
        // -----------------------------------------------
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
