using HRMS.Data;
using HRMS.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace HRMS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var today = DateTime.Now.Date;
            var tomorrow = today.AddDays(1);
            var employees = _context.Employees.ToList();

            // 🎉 Build celebrations model
            var model = new CelebrationViewModel
            {
                TodaysBirthdays = employees
                    .Where(e => e.DOB_Date.HasValue &&
                                e.DOB_Date.Value.Month == today.Month &&
                                e.DOB_Date.Value.Day == today.Day)
                    .ToList(),

                TomorrowsBirthdays = employees
                    .Where(e => e.DOB_Date.HasValue &&
                                e.DOB_Date.Value.Month == tomorrow.Month &&
                                e.DOB_Date.Value.Day == tomorrow.Day)
                    .ToList(),

                TodaysAnniversaries = employees
                    .Where(e => e.JoiningDate.HasValue &&
                                e.JoiningDate.Value.Month == today.Month &&
                                e.JoiningDate.Value.Day == today.Day)
                    .Select(e => new Employee
                    {
                        Name = e.Name,
                        Position = $"{today.Year - e.JoiningDate.Value.Year} Year Work Anniversary"
                    })
                    .ToList(),

                TomorrowsAnniversaries = employees
                    .Where(e => e.JoiningDate.HasValue &&
                                e.JoiningDate.Value.Month == tomorrow.Month &&
                                e.JoiningDate.Value.Day == tomorrow.Day)
                    .Select(e => new Employee
                    {
                        Name = e.Name,
                        Position = $"{tomorrow.Year - e.JoiningDate.Value.Year} Year Work Anniversary"
                    })
                    .ToList(),

                TotalEmployees = employees.Count
            };

            // 📊 Department Distribution
            var departmentCounts = employees
                .GroupBy(e => e.Department)
                .Select(g => new { Department = g.Key, Count = g.Count() })
                .ToList();

            ViewBag.DepartmentLabels = departmentCounts.Select(d => d.Department).ToArray();
            ViewBag.DepartmentValues = departmentCounts.Select(d => d.Count).ToArray();

            return View(model);
        }
    }
}
