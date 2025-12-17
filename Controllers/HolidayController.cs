using HRMS.Data;
using HRMS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Controllers
{
    public class HolidayController : Controller
    {
        private readonly ApplicationDbContext _context;
        

        public HolidayController(ApplicationDbContext context)
        {
            _context = context;
            
        }

        public async Task<IActionResult> Index()
        {
            var empId = HttpContext.Session.GetInt32("EmployeeId");
            if (empId == null)
                return RedirectToAction("Login", "Account");

            // ✅ Await FindAsync
            var emp = await _context.Employees.FindAsync(empId);

            if (emp == null)
                return RedirectToAction("Login", "Account");

            // ✅ SAFE string cast
            ViewBag.UserRole = emp.Role?.ToString();
            return View();
        }
    }
}
