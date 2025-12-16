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

        // ============================
        // LOGIN (GET)
        // ============================
        [HttpGet]
        public IActionResult Login()
        {

            return View(new LoginViewModel());
        }

        // ============================
        // LOGIN (POST)
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Find employee (HR or normal employee)
            var emp = _context.Employees
                .FirstOrDefault(e =>
                    e.EmployeeCode == model.UserId &&
                    e.Password == model.Password);

            if (emp == null)
            {
                ViewBag.Error = "Invalid Employee Code or Password.";
                return View(model);
            }


            // Normalize Role
            string role = emp.Role?.Trim() ?? "Employee";

            // Normalize role formatting (optional)
            if (role.Equals("admin", StringComparison.OrdinalIgnoreCase))
                role = "HR";



            // Store session
            HttpContext.Session.SetString("Role", role);
            HttpContext.Session.SetInt32("EmployeeId", emp.Id);
            HttpContext.Session.SetString("EmployeeName", emp.Name);
            HttpContext.Session.SetString("EmpCode", emp.EmployeeCode);

            // ===========================================
            // ROLE-BASED REDIRECT
            // ===========================================
             role = (emp.Role ?? "Employee").Trim();

            if (role.Equals("HR", StringComparison.OrdinalIgnoreCase) ||
                role.Equals("VP", StringComparison.OrdinalIgnoreCase) ||
                role.Equals("GM", StringComparison.OrdinalIgnoreCase) ||
                role.Equals("Director", StringComparison.OrdinalIgnoreCase) )
                //role.Equals("Manager", StringComparison.OrdinalIgnoreCase)
            {
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Dashboard", "Employees");

        }

        // ============================
        // LOGOUT
        // ============================
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}
