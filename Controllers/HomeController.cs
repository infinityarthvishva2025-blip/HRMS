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

            // ---------------- ACTIVE EMPLOYEES ----------------
            var activeEmployees = _context.Employees
                .Where(e => e.Status == "Active")
                .AsNoTracking()
                .ToList();

            var allEmployees = _context.Employees.AsNoTracking().ToList();

            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            // ---------------- CELEBRATION MODEL ----------------
            var model = new CelebrationViewModel
            {
                Employee = loggedEmployee,
                TotalEmployees = allEmployees.Count,
                ActiveEmployees = activeEmployees.Count,

                TodaysBirthdays = activeEmployees.Where(e =>
                    e.DOB_Date.HasValue &&
                    e.DOB_Date.Value.Month == today.Month &&
                    e.DOB_Date.Value.Day == today.Day).ToList(),

                TomorrowsBirthdays = activeEmployees.Where(e =>
                    e.DOB_Date.HasValue &&
                    e.DOB_Date.Value.Month == tomorrow.Month &&
                    e.DOB_Date.Value.Day == tomorrow.Day).ToList(),

                TodaysAnniversaries = activeEmployees.Where(e =>
                    e.JoiningDate.HasValue &&
                    e.JoiningDate.Value.Month == today.Month &&
                    e.JoiningDate.Value.Day == today.Day).ToList(),

                TomorrowsAnniversaries = activeEmployees.Where(e =>
                    e.JoiningDate.HasValue &&
                    e.JoiningDate.Value.Month == tomorrow.Month &&
                    e.JoiningDate.Value.Day == tomorrow.Day).ToList(),
            };

            // ---------------- TODAY'S ATTENDANCE ----------------
            var todaysAttendance = _context.Attendances
                .Where(a => a.Date == today)
                .AsNoTracking()
                .ToList();

            // PRESENT TODAY
            ViewBag.PresentToday = todaysAttendance
     .Where(a =>
         a.InTime != null &&                       // ✅ Checked In
         activeEmployees.Any(e => e.EmployeeCode == a.Emp_Code))
     .Select(a => a.Emp_Code)
     .Distinct()
     .Count();


            // ABSENT TODAY
            ViewBag.AbsentToday = activeEmployees.Count - ViewBag.PresentToday;

            // NOT CHECKED OUT
            ViewBag.NotCheckedOutToday = todaysAttendance
                .Where(a => activeEmployees.Any(e => e.EmployeeCode == a.Emp_Code)
                            && a.OutTime == null)
                .Count();


            // ---------------- RECENT 5 ACTIVE EMPLOYEES (NO ATTENDANCE CONDITION) ----------------
            var recentEmployees = _context.Employees
      .AsNoTracking()
      .Where(e => e.Status == "Active" && e.JoiningDate.HasValue)
      .OrderByDescending(e => e.JoiningDate.Value)
      .ThenByDescending(e => e.EmployeeCode) // secondary order
      .Take(5)
      .Select(emp => new RecentEmployeeViewModel
      {
          Employee = emp
      })
      .ToList();

            ViewBag.RecentEmployees = recentEmployees;



            // ---------------- DEPARTMENT DISTRIBUTION ----------------
            var departmentGroups = allEmployees
                .GroupBy(e => e.Department)
                .Select(g => new { Department = g.Key, Count = g.Count() })
                .ToList();

            ViewBag.DepartmentLabels = departmentGroups.Select(d => d.Department).ToList();
            ViewBag.DepartmentValues = departmentGroups.Select(d => d.Count).ToList();

            return View(model);
        }
    }
}
