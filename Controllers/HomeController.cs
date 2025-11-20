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

            // =====================================================
            // 🎉 CELEBRATION DATA (BIRTHDAYS + ANNIVERSARIES)
            // =====================================================
            var model = new CelebrationViewModel
            {
                TotalEmployees = employees.Count,

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
                    .ToList(),

                TomorrowsAnniversaries = employees
                    .Where(e => e.JoiningDate.HasValue &&
                                e.JoiningDate.Value.Month == tomorrow.Month &&
                                e.JoiningDate.Value.Day == tomorrow.Day)
                    .ToList()
            };


            // =====================================================
            // 🟢 TODAY’S ATTENDANCE
            // =====================================================
            var todaysAttendance = _context.Attendances
                .Where(a => a.Date == today)
                .AsNoTracking()
                .ToList();

            int presentCount = todaysAttendance
                .Select(a => a.Emp_Code)
                .Distinct()
                .Count();

            int absentCount = employees.Count - presentCount;

            int notCheckedOutCount = todaysAttendance
                .Count(a => a.OutTime == null);

            ViewBag.PresentToday = presentCount;
            ViewBag.AbsentToday = absentCount;
            ViewBag.NotCheckedOutToday = notCheckedOutCount;


            // =====================================================
            // 🆕 RECENT EMPLOYEES (LAST 5)
            // =====================================================
            var last5Employees = _context.Employees
                .AsNoTracking()
                .OrderByDescending(e => e.JoiningDate ?? DateTime.MinValue)
                .Take(5)
                .ToList();

            var recentEmployees = last5Employees
                .Select(e => new RecentEmployeeViewModel
                {
                    Employee = e,
                    Attendance = todaysAttendance.FirstOrDefault(a => a.Emp_Code == e.EmployeeCode)
                })
                .ToList();

            ViewBag.RecentEmployees = recentEmployees;


            // =====================================================
            // 📊 DEPARTMENT CHART
            // =====================================================
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
