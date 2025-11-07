using HRMS.Data;
using HRMS.Models;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Inject the context via constructor
        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            // Fetch employees from database
            var employees = _context.Employees.ToList();

            // Today's birthdays
            var todaysBirthdays = employees
                .Where(e => e.DOB_Date.Day == today.Day && e.DOB_Date.Month == today.Month)
                .ToList();

            // Tomorrow's birthdays
            var tomorrowsBirthdays = employees
                .Where(e => e.DOB_Date.Day == tomorrow.Day && e.DOB_Date.Month == tomorrow.Month)
                .ToList();

            var model = new CelebrationViewModel
            {
                TodaysBirthdays = todaysBirthdays,
                TomorrowsBirthdays = tomorrowsBirthdays
            };

            return View(model);
        }
    }
}
