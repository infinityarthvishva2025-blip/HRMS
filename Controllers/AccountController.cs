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

            // ================================
            // 1️⃣ HR LOGIN (STATIC)
            // ================================
            if (model.UserId.Equals("HR001", StringComparison.OrdinalIgnoreCase)
                && model.Password == "admin123")
            {
                HttpContext.Session.SetString("Role", "HR");
                HttpContext.Session.SetString("HrName", "Admin HR");
                return RedirectToAction("Index", "Home");
            }

            // ================================
            // 2️⃣ EMPLOYEE LOGIN USING EmployeeCode + Password
            // ================================
            var emp = _context.Employees
                .FirstOrDefault(e =>
                    e.EmployeeCode == model.UserId &&
                    e.Password == model.Password);

            if (emp == null)
            {
                ViewBag.Error = "Invalid Employee Code or Password.";
                return View(model);
            }

            // Save session for the logged employee
            HttpContext.Session.SetString("Role", "Employee");
            HttpContext.Session.SetInt32("EmployeeId", emp.Id);
            HttpContext.Session.SetString("EmployeeName", emp.Name);

            // Run auto checkout cleanup
            AutoCheckoutForgottenRecords(emp.Id);

            // Redirect only this employee to HIS dashboard
            return RedirectToAction("Dashboard", "Employees");
        }



        // =============================================
        // LOGOUT
        // =============================================
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();   // Clear all login session values
            return RedirectToAction("Login", "Account"); // Redirect user back to login page
        }


        private void AutoCheckoutForgottenRecords(int employeeId)
        {
            var yesterday = DateTime.Today.AddDays(-1);

            var pending = _context.Attendances
                .Where(a =>
                    a.EmployeeId == employeeId &&
                    a.CheckInTime.HasValue &&
                    a.CheckInTime.Value.Date == yesterday &&
                    a.CheckOutTime == null)
                .ToList();

            if (pending.Count == 0)
                return;

            foreach (var record in pending)
            {
                record.CheckOutTime = yesterday.Date.AddHours(23).AddMinutes(59); // 11:59 PM
                record.CheckoutStatus = "Auto Checkout";
            }

            _context.SaveChanges();
        }

    }
}
