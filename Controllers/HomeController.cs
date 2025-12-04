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
            var empCode = HttpContext.Session.GetString("EmpCode");

            var loggedEmployee = _context.Employees
                .FirstOrDefault(e => e.EmployeeCode == empCode);

            // GET ONLY ACTIVE EMPLOYEES
            var activeEmployees = _context.Employees
                .Where(e => e.Status == "Active")
                .AsNoTracking()
                .ToList();

            // If you need all employees for birthday/anniversary, keep full list
            var allEmployees = _context.Employees.AsNoTracking().ToList();

            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var model = new CelebrationViewModel
            {
                Employee = loggedEmployee,

                // ONLY ACTIVE EMPLOYEES COUNT
                TotalEmployees = activeEmployees.Count,

                // Birthdays/Anniversaries - Based on ALL employees (not only active)
                TodaysBirthdays = allEmployees.Where(e =>
                    e.DOB_Date.HasValue &&
                    e.DOB_Date.Value.Month == today.Month &&
                    e.DOB_Date.Value.Day == today.Day).ToList(),

                TomorrowsBirthdays = allEmployees.Where(e =>
                    e.DOB_Date.HasValue &&
                    e.DOB_Date.Value.Month == tomorrow.Month &&
                    e.DOB_Date.Value.Day == tomorrow.Day).ToList(),

                TodaysAnniversaries = allEmployees.Where(e =>
                    e.JoiningDate.HasValue &&
                    e.JoiningDate.Value.Month == today.Month &&
                    e.JoiningDate.Value.Day == today.Day).ToList(),

                TomorrowsAnniversaries = allEmployees.Where(e =>
                    e.JoiningDate.HasValue &&
                    e.JoiningDate.Value.Month == tomorrow.Month &&
                    e.JoiningDate.Value.Day == tomorrow.Day).ToList(),
            };


            // ----------- TODAY'S ATTENDANCE ONLY FOR ACTIVE EMPLOYEES -------------- //

            var todaysAttendance = _context.Attendances
                .Where(a => a.Date == today)
                .ToList();

            // PRESENT = Active employees who have attendance today
            ViewBag.PresentToday = todaysAttendance
                .Where(a => activeEmployees.Any(emp => emp.EmployeeCode == a.Emp_Code))
                .Select(a => a.Emp_Code)
                .Distinct()
                .Count();

            // ABSENT = Active employees - present employees
            ViewBag.AbsentToday = activeEmployees.Count - ViewBag.PresentToday;

            // NOT CHECKED OUT = Active employees who came but not OutTime
            ViewBag.NotCheckedOutToday = todaysAttendance
                .Where(a => activeEmployees.Any(emp => emp.EmployeeCode == a.Emp_Code)
                            && a.OutTime == null)
                .Count();


            // Recent Employees (Only ACTIVE employees)
            var recentActive = activeEmployees
                .OrderByDescending(e => e.JoiningDate ?? DateTime.MinValue)
                .Take(5)
                .ToList();

            var recentList = recentActive
                .Select(emp => new RecentEmployeeViewModel
                {
                    Employee = emp,
                    Attendance = todaysAttendance.FirstOrDefault(a => a.Emp_Code == emp.EmployeeCode)
                })
                .ToList();

            ViewBag.RecentEmployees = recentList;
            var employees = _context.Employees;
            var departmentGroups = employees
               .GroupBy(e => e.Department)
               .Select(g => new { Department = g.Key, Count = g.Count() })
               .ToList();

            ViewBag.DepartmentLabels = departmentGroups.Select(d => d.Department).ToList();
            ViewBag.DepartmentValues = departmentGroups.Select(d => d.Count).ToList();

            return View(model);
        }

        //public IActionResult Index()
        //{
        //    var empCode = HttpContext.Session.GetString("EmpCode");

        //    var loggedEmployee = _context.Employees
        //        .FirstOrDefault(e => e.EmployeeCode == empCode);

        //    var employees = _context.Employees
        //        .AsNoTracking()
        //        .ToList();

        //    var today = DateTime.Today;
        //    var tomorrow = today.AddDays(1);

        //    var model = new CelebrationViewModel
        //    {
        //        Employee = loggedEmployee,   // ← Add this

        //        TotalEmployees = employees.Count,

        //        TodaysBirthdays = employees
        //            .Where(e => e.DOB_Date.HasValue &&
        //                        e.DOB_Date.Value.Month == today.Month &&
        //                        e.DOB_Date.Value.Day == today.Day)
        //            .ToList(),

        //        TomorrowsBirthdays = employees
        //            .Where(e => e.DOB_Date.HasValue &&
        //                        e.DOB_Date.Value.Month == tomorrow.Month &&
        //                        e.DOB_Date.Value.Day == tomorrow.Day)
        //            .ToList(),

        //        TodaysAnniversaries = employees
        //            .Where(e => e.JoiningDate.HasValue &&
        //                        e.JoiningDate.Value.Month == today.Month &&
        //                        e.JoiningDate.Value.Day == today.Day)
        //            .ToList(),

        //        TomorrowsAnniversaries = employees
        //            .Where(e => e.JoiningDate.HasValue &&
        //                        e.JoiningDate.Value.Month == tomorrow.Month &&
        //                        e.JoiningDate.Value.Day == tomorrow.Day)
        //            .ToList(),
        //    };

        //    // Today Attendance
        //    var todaysAtt = _context.Attendances
        //        .Where(a => a.Date == today)
        //        .ToList();

        //    ViewBag.PresentToday = todaysAtt.Select(a => a.Emp_Code).Distinct().Count();
        //    ViewBag.AbsentToday = employees.Count - ViewBag.PresentToday;
        //    ViewBag.NotCheckedOutToday = todaysAtt.Count(a => a.OutTime == null);

        //    // Recent Employees
        //    var last5 = employees
        //        .OrderByDescending(e => e.JoiningDate ?? DateTime.MinValue)
        //        .Take(5)
        //        .ToList();

        //    var recentList = last5
        //        .Select(emp => new RecentEmployeeViewModel
        //        {
        //            Employee = emp,
        //            Attendance = todaysAtt.FirstOrDefault(a => a.Emp_Code == emp.EmployeeCode)
        //        })
        //        .ToList();

        //    ViewBag.RecentEmployees = recentList;

        //    return View(model);
        //}
    }
}
