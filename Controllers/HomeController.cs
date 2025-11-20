using HRMS.Data;
using HRMS.Models;
using HRMS.Models.ViewModels;
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
            var employees = _context.Employees
                .AsNoTracking()
                .ToList();

            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            // Celebration Model
            var model = new CelebrationViewModel
            {
                TotalEmployees = employees.Count,
                TodaysBirthdays = employees.Where(e => e.DOB_Date?.Date == today).ToList(),
                TomorrowsBirthdays = employees.Where(e => e.DOB_Date?.Date == tomorrow).ToList(),
                TodaysAnniversaries = employees.Where(e => e.JoiningDate?.Date == today).ToList(),
                TomorrowsAnniversaries = employees.Where(e => e.JoiningDate?.Date == tomorrow).ToList()
            };

            // Today Attendance
            var todaysAtt = _context.Attendances
                .Where(a => a.Date == today)
                .ToList();

            ViewBag.PresentToday = todaysAtt.Select(a => a.EmpCode).Distinct().Count();
            ViewBag.AbsentToday = employees.Count - ViewBag.PresentToday;
            ViewBag.NotCheckedOutToday = todaysAtt.Count(a => a.OutTime == null);

            // Recent Employees
            var last5 = employees
                .OrderByDescending(e => e.JoiningDate ?? DateTime.MinValue)
                .Take(5)
                .ToList();

            var recentList = last5
                .Select(emp => new RecentEmployeeViewModel
                {
                    Employee = emp,
                    Attendance = todaysAtt.FirstOrDefault(a => a.EmpCode == emp.EmployeeCode)
                })
                .ToList();

            ViewBag.RecentEmployees = recentList;

            return View(model);
        }
    }
}
