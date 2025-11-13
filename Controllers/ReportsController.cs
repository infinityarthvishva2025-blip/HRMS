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
            // Example 3: Payroll summary by month
            var payrollReport = _context.Payrolls
                .GroupBy(p => p.MonthYear)
                .Select(g => new { Month = g.Key, Total = g.Sum(p => p.NetPay) })
                .ToList();
            ViewBag.payrollReport = payrollReport;
            ViewBag.LeaveReport = leaveReport;


            var data = new List<Payroll>
    {
        new Payroll { MonthYear = "Jan", NetPay = 350000 },
        new Payroll { MonthYear = "Feb", NetPay = 370000 },
        new Payroll { MonthYear = "Mar", NetPay = 420000 },
        new Payroll { MonthYear = "Apr", NetPay = 390000 },
        new Payroll { MonthYear = "May", NetPay = 450000 },
        new Payroll { MonthYear = "Jun", NetPay = 470000 }
    };
            var approved = _context.Leaves.Count(x => x.Status == "Approved");
            var pending = _context.Leaves.Count(x => x.Status == "Pending");
            var rejected = _context.Leaves.Count(x => x.Status == "Rejected");

            var data1 = new List<Leave>
    {
        new Leave { Status = "Approved", Count = approved },
        new Leave { Status = "Pending", Count = pending },
        new Leave { Status = "Rejected", Count = rejected }
    };


            return Json(data, data1);

            //return Json(data);
            //return View();
        }
    }


}
