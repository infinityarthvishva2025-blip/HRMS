using HRMS.Data;
using HRMS.Models;
using HRMS.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =============================================
        // LOGIN (GET)
        // =============================================
        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        // =============================================
        // LOGIN (POST)
        // =============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // =============================================
            // 1️⃣ HR LOGIN (HARDCODED)
            // =============================================
            const string defaultHrId = "HR001";
            const string defaultHrPassword = "admin123";

            if (model.UserId.Equals(defaultHrId, StringComparison.OrdinalIgnoreCase)
                && model.Password == defaultHrPassword)
            {
                HttpContext.Session.SetString("Role", "HR");
                HttpContext.Session.SetString("HrName", "Admin HR");
                return RedirectToAction("Index", "Home");
            }

            // =============================================
            // 2️⃣ EMPLOYEE LOGIN
            // =============================================
            var emp = _context.Employees
                .FirstOrDefault(e => e.EmployeeCode == model.UserId && e.Password == model.Password);

            if (emp == null)
            {
                ViewBag.Error = "Invalid ID or Password.";
                return View(model);
            }

            // Store session
            HttpContext.Session.SetString("Role", "Employee");
            HttpContext.Session.SetInt32("EmployeeId", emp.Id);
            HttpContext.Session.SetString("EmployeeName", emp.Name);
           

            // =============================================
            // 3️⃣ AUTO CHECKOUT LOGIC (RUNS AT LOGIN)
            // =============================================
            AutoCheckoutForgottenRecords(emp.Id);

            // Redirect to employee dashboard
            return RedirectToAction("Dashboard", "Employees");
        }

        // =============================================
        // AUTO CHECK-OUT FUNCTION
        // =============================================
        private void AutoCheckoutForgottenRecords(int employeeId)
        {
            var yesterday = DateTime.Today.AddDays(-1);

            // Find attendance records from yesterday where employee forgot to checkout
            var pending = _context.Attendances
                .Where(a =>
                    a.EmployeeId == employeeId &&
                    a.CheckInTime.Value.Date == yesterday &&
                    a.CheckOutTime == null)
                .ToList();

            if (pending.Count == 0)
                return;

            foreach (var record in pending)
            {
                record.CheckOutTime = yesterday.Date.AddHours(23).AddMinutes(59); // 11:59 PM
                record.CheckoutStatus = "Auto Checkout";  // <-- Make sure this column exists
            }

            _context.SaveChanges();
        }

        // =============================================
        // LOGOUT
        // =============================================
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();   // Clear all login session values
            return RedirectToAction("Login", "Account"); // Redirect user back to login page
        }
    }
}
