using HRMS.Data;
using HRMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            // ✅ Fetch all employees for stats and celebrations
            var employees = _context.Employees.AsNoTracking().ToList();

            // ✅ Get today's and tomorrow's dates
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            // ✅ Build CelebrationViewModel
            var model = new CelebrationViewModel
            {
                TotalEmployees = employees.Count,

                // 🎂 Today's Birthdays
                TodaysBirthdays = employees
                    .Where(e => e.DOB_Date.HasValue &&
                                e.DOB_Date.Value.Month == today.Month &&
                                e.DOB_Date.Value.Day == today.Day)
                    .ToList(),

                // 🎂 Tomorrow's Birthdays
                TomorrowsBirthdays = employees
                    .Where(e => e.DOB_Date.HasValue &&
                                e.DOB_Date.Value.Month == tomorrow.Month &&
                                e.DOB_Date.Value.Day == tomorrow.Day)
                    .ToList(),

                // 🎉 Today's Anniversaries
                TodaysAnniversaries = employees
                    .Where(e => e.JoiningDate.HasValue &&
                                e.JoiningDate.Value.Month == today.Month &&
                                e.JoiningDate.Value.Day == today.Day)
                    .ToList(),

                // 🎊 Tomorrow's Anniversaries
                TomorrowsAnniversaries = employees
                    .Where(e => e.JoiningDate.HasValue &&
                                e.JoiningDate.Value.Month == tomorrow.Month &&
                                e.JoiningDate.Value.Day == tomorrow.Day)
                    .ToList()
            };

            // ✅ Recent Employees (latest 5 by joining date)
            var recentEmployees = _context.Employees
                .AsNoTracking()
                .OrderByDescending(e => e.JoiningDate ?? DateTime.MinValue)
                .Take(5)
                .ToList();

            ViewBag.RecentEmployees = recentEmployees;

            // ✅ Department-wise distribution for Chart
            var departmentGroups = employees
                .Where(e => !string.IsNullOrEmpty(e.Department))
                .GroupBy(e => e.Department)
                .Select(g => new { Department = g.Key, Count = g.Count() })
                .ToList();

            ViewBag.DepartmentLabels = departmentGroups.Select(d => d.Department).ToList();
            ViewBag.DepartmentValues = departmentGroups.Select(d => d.Count).ToList();

            return View(model);
        }
    }
}
