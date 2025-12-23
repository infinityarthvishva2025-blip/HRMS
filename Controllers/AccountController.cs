using HRMS.Data;
using HRMS.Models;
using HRMS.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

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
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var emp = _context.Employees.FirstOrDefault(e =>
                e.EmployeeCode == model.UserId &&
                e.Password == model.Password);

            if (emp == null)
            {
                ViewBag.Error = "Invalid Employee Code or Password.";
                return View(model);
            }

            // ============================
            // NORMALIZE ROLE
            // ============================
            string role = emp.Role?.Trim() ?? "Employee";
            if (role.Equals("admin", StringComparison.OrdinalIgnoreCase))
                role = "HR";

            // ============================
            // KEEP EXISTING SESSION (SAFE)
            // ============================
            HttpContext.Session.SetString("Role", role);
            HttpContext.Session.SetInt32("EmployeeId", emp.Id);
            HttpContext.Session.SetString("EmployeeName", emp.Name);
            HttpContext.Session.SetString("EmpCode", emp.EmployeeCode);

            // ============================
            // COOKIE AUTH (EMPLOYEE ID BASED)
            // ============================
            var claims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, emp.Id.ToString()),
    new Claim(ClaimTypes.Role, role),
    new Claim("EmployeeCode", emp.EmployeeCode),
    new Claim("EmployeeName", emp.Name)
};

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

            // ============================
            // REDIRECT (UNCHANGED LOGIC)
            // ============================
            if (role.Equals("HR", StringComparison.OrdinalIgnoreCase) ||
                role.Equals("VP", StringComparison.OrdinalIgnoreCase) ||
                role.Equals("GM", StringComparison.OrdinalIgnoreCase) ||
                role.Equals("Director", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Dashboard", "Employees");
        }

        // ============================
        // LOGOUT
        // ============================
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();

            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login", "Account");
        }
    }
}
