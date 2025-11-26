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

            // Save employee login session
            HttpContext.Session.SetString("EmployeeCode", emp.EmployeeCode);

            HttpContext.Session.SetString("Role", "Employee");
            HttpContext.Session.SetInt32("EmployeeId", emp.Id);
            HttpContext.Session.SetString("EmployeeName", emp.Name);
            HttpContext.Session.SetString("EmpCode", emp.EmployeeCode);

            // Auto checkout yesterday's records for this employee
            //AutoCheckoutForgottenRecords(emp.EmployeeCode);

            // Redirect employee to dashboard
            return RedirectToAction("Dashboard", "Employees");
        }



        // =============================================
        // LOGOUT
        // =============================================
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }


        // =============================================
        // AUTO CHECKOUT FOR FORGOTTEN RECORDS
        // =============================================
        //private void AutoCheckoutForgottenRecords(string empCode)
        //{
        //    var yesterday = DateTime.Today.AddDays(-1);

        //    var pending = _context.Attendances
        //        .Where(a =>
        //            a.Emp_Code == empCode &&
        //            a.Date == yesterday &&
        //            a.InTime != null &&
        //            a.OutTime == null
        //        )
        //        .ToList();

        //    if (pending.Count == 0)
        //        return;

        //    foreach (var record in pending)
        //    {
        //        // Set auto-checkout at 11:59 PM
        //        record.OutTime = yesterday.AddHours(23).AddMinutes(59);

        //        // Calculate hours
        //        if (record.InTime.HasValue)
        //            record.Total_Hours = record.OutTime.Value - record.InTime.Value;

        //        // Set updated status
        //        record.Status = "Auto Checkout";
        //    }

        //    _context.SaveChanges();
        //}

    }
}
