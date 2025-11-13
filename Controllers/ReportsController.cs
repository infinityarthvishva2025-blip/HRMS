using HRMS.Data;
using HRMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }
        //public IActionResult Index()
        //{
        //    // Example 1: Employee count by department
        //    var employeeReport = _context.Employees
        //        .GroupBy(e => e.Department)
        //        .Select(g => new { Department = g.Key, Count = g.Count() })
        //        .ToList();

        //    // Example 2: Leave requests by status
        //    var leaveReport = _context.Leaves
        //        .GroupBy(l => l.Status)
        //        .Select(g => new { Status = g.Key, Count = g.Count() })
        //        .ToList();

        //    // Example 3: Payroll summary by month
        //    var payrollReport = _context.Payrolls
        //        .GroupBy(p => p.Month)
        //        .Select(g => new { Month = g.Key, Total = g.Sum(p => p.NetSalary) })
        //        .ToList();

        //    // Example 4: Attendance summary
        //    //var attendanceReport = _context.Attendances
        //    //    .GroupBy(a => a.Status)
        //    //    .Select(g => new { Status = g.Key, Count = g.Count() })
        //    //    .ToList();

        //    ViewBag.EmployeeReport = employeeReport;
        //    ViewBag.LeaveReport = leaveReport;
        //    ViewBag.PayrollReport = payrollReport;
        //    //ViewBag.AttendanceReport = attendanceReport;

        //    return View();
        //}



        public IActionResult Index()
        {
            // Fetch all employees for stats and celebrations
            var employees = _context.Employees;
            // Prepare department distribution chart data
            var departmentGroups = employees
                .GroupBy(e => e.Department)
                .Select(g => new { Department = g.Key, Count = g.Count() })
                .ToList();

            ViewBag.DepartmentLabels = departmentGroups.Select(d => d.Department).ToList();
            ViewBag.DepartmentValues = departmentGroups.Select(d => d.Count).ToList();
            // Example 2: Leave requests by status
            var leaveReport = _context.Leaves
                .GroupBy(l => l.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToList();
            ViewBag.LeaveReport = leaveReport;
            return View();
        }
    }
}
