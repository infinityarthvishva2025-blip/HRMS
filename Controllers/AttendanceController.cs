using HRMS.Data;
using HRMS.Models;
using HRMS.Models.ViewModels;
using HRMS.Services;
using HRMS.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Linq;

namespace HRMS.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AttendanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // EMPLOYEE PANEL
        // =========================================================
        public IActionResult EmployeePanel()
        {
            string empCode = HttpContext.Session.GetString("EmpCode");
            if (string.IsNullOrEmpty(empCode))
                return RedirectToAction("Login", "Account");

            var today = DateTime.Today;

            var record = _context.Attendances
                .FirstOrDefault(a => a.Emp_Code == empCode && a.Date == today);

            return View(record);
        }

        // =========================================================
        // CHECK IN
        // =========================================================
        public IActionResult CheckIn()
        {
            string empCode = HttpContext.Session.GetString("EmpCode");
            if (string.IsNullOrEmpty(empCode))
                return RedirectToAction("Login", "Account");

            var today = DateTime.Today;

            bool exists = _context.Attendances
                .Any(a => a.Emp_Code == empCode && a.Date == today);

            if (!exists)
            {
                var rec = new Attendance
                {
                    Emp_Code = empCode,
                    Date = today,
                    Status = "P",
                    InTime = DateTime.Now.TimeOfDay,
                    OutTime = null,
                    Total_Hours = 0
                };

                _context.Attendances.Add(rec);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(EmployeePanel));
        }

        // =========================================================
        // CHECK OUT
        // =========================================================
        public IActionResult CheckOut()
        {
            string empCode = HttpContext.Session.GetString("EmpCode");

            if (string.IsNullOrEmpty(empCode))
                return RedirectToAction("Login", "Account");

            var today = DateTime.Today;

            var rec = _context.Attendances
                .FirstOrDefault(a => a.Emp_Code == empCode && a.Date == today);

            if (rec != null && rec.OutTime == null)
            {
                rec.OutTime = DateTime.Now.TimeOfDay;

                // TOTAL HOURS
                rec.Total_Hours = CalculateTotalHours(rec.InTime, rec.OutTime);

                _context.SaveChanges();
            }

            return RedirectToAction(nameof(EmployeePanel));
        }

        private decimal CalculateTotalHours(TimeSpan? inTime, TimeSpan? outTime)
        {
            if (!inTime.HasValue || !outTime.HasValue)
                return 0;

            TimeSpan diff = outTime.Value - inTime.Value;

            if (diff.TotalHours < 0)
                return 0;

            return (decimal)diff.TotalHours;
        }


        // =========================================================
        // SUMMARY REDIRECT
        // =========================================================
        public IActionResult MySummary()
        {
            string empCode = HttpContext.Session.GetString("EmpCode");

            if (string.IsNullOrEmpty(empCode))
                return RedirectToAction("Login", "Account");

            return RedirectToAction(nameof(EmployeeSummary), new { empCode });
        }

        // =========================================================
        // EMPLOYEE SUMMARY PAGE
        // =========================================================
        [HttpGet]
        public IActionResult EmployeeSummary(int employeeId, DateTime? from = null, DateTime? to = null)
        {
            if (employeeId <= 0)
                return BadRequest("Invalid employee ID.");

            // Get employee record
            var emp = _context.Employees.FirstOrDefault(e => e.Id == employeeId);
            if (emp == null)
                return NotFound("Employee not found.");

            string empCode = emp.EmployeeCode;   // IMPORTANT: Attendance table uses Emp_Code

            // ---------------------------------------
            // DEFAULT DATE RANGE → CURRENT MONTH
            // ---------------------------------------
            if (!from.HasValue)
                from = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            if (!to.HasValue)
                to = DateTime.Now.Date;

            DateTime startDate = from.Value.Date;
            DateTime endDate = to.Value.Date.AddDays(1).AddSeconds(-1); // include entire day

            // ---------------------------------------
            // GET ATTENDANCE RECORDS FOR EMPLOYEE
            // ---------------------------------------
            var attendanceRecords = _context.Attendances
                .Where(a => a.Emp_Code == empCode)     // FIXED 🔥 use Emp_Code
                .Where(a => a.Date >= startDate && a.Date <= endDate)
                .OrderByDescending(a => a.Date)
                .ToList();

            // ---------------------------------------
            // CALCULATE SUMMARY
            // ---------------------------------------
            TimeSpan shiftStart = new TimeSpan(9, 30, 0);
            TimeSpan shiftEnd = new TimeSpan(18, 0, 0);

            var summary = new EmployeeAttendanceSummaryViewModel
            {
                Employee = emp,
                AttendanceRecords = attendanceRecords,
                FromDate = startDate,
                ToDate = endDate,

                TotalDays = attendanceRecords.Count,

                TotalLateDays = attendanceRecords.Count(a =>
                    a.InTime.HasValue && a.InTime.Value > shiftStart),

                TotalEarlyLeaveDays = attendanceRecords.Count(a =>
                    a.OutTime.HasValue && a.OutTime.Value < shiftEnd),

                AverageWorkingHours = attendanceRecords
                    .Where(a => a.InTime.HasValue && a.OutTime.HasValue)
                    .Select(a => (a.OutTime.Value - a.InTime.Value).TotalHours)
                    .DefaultIfEmpty(0)
                    .Average()
                    .ToString("0.0")
            };

            return View(summary);
        }




        // =========================================================
        // HR LIST PAGE (FILTER + SEARCH)
        // =========================================================
        public IActionResult Index(string search, DateTime? fromDate, DateTime? toDate, string status = "All")
        {
            var attendance = _context.AttendanceRecords.AsQueryable();

            // 🟢 Default: current month
            if (!fromDate.HasValue && !toDate.HasValue)
            {
                var firstDay = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var lastDay = firstDay.AddMonths(1).AddDays(-1);

                fromDate = firstDay;
                toDate = lastDay;
            }

            // 🧭 Apply date range
            attendance = attendance.Where(a => a.Date >= fromDate && a.Date <= toDate);

            // 🔍 Search filter
            if (!string.IsNullOrEmpty(search))
                attendance = attendance.Where(a => a.Emp_Code.Contains(search));

            // 📋 Status filter
            if (status == "NotCheckedOut")
                attendance = attendance.Where(a => a.InTime != null && a.OutTime == null);
            else if (status == "Completed")
                attendance = attendance.Where(a => a.InTime != null && a.OutTime != null);

            // 🌐 ViewBag for form prefill
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
            ViewBag.Search = search;
            ViewBag.Status = status;

            // 📊 Projection to ViewModel
            var list = attendance
                .OrderByDescending(a => a.Date)
                .Select(a => new AttendanceIndexVm
                {
                    Emp_Code = a.Emp_Code,
                    AttDate = a.Date,
                    InTime = a.InTime,
                    OutTime = a.OutTime,
                    TotalHours = (a.InTime != null && a.OutTime != null)
                        ? (a.OutTime.Value - a.InTime.Value).ToString(@"hh\:mm")
                        : "--"
                })
                .ToList();

            // 👥 Load employees for name display
            ViewBag.EmployeeList = _context.Employees.ToList();

            return View(list);
        }



        private bool IsLate(Attendance a)
        {
            bool presentType = a.Status == "P" || a.Status == "WOP" ||
                               a.Status == "½P" || a.Status == "P½";

            if (!presentType || !a.InTime.HasValue)
                return false;

            var day = a.Date.DayOfWeek;

            if (day == DayOfWeek.Sunday)
                return false;

            TimeSpan cutoff = (day == DayOfWeek.Saturday)
                ? new TimeSpan(10, 15, 0)
                : new TimeSpan(9, 45, 0);

            return a.InTime.Value > cutoff;
        }

        // =========================================================
        // EXPORT TO EXCEL
        // =========================================================
        [HttpGet]
        public IActionResult ExportFiltered(string search, DateTime? fromDate, DateTime? toDate, string status)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var data = _context.Attendances.AsQueryable();

            if (fromDate.HasValue)
                data = data.Where(a => a.Date >= fromDate.Value.Date);

            if (toDate.HasValue)
                data = data.Where(a => a.Date <= toDate.Value.Date);

            if (!string.IsNullOrEmpty(search))
                data = data.Where(a => a.Emp_Code.Contains(search));

            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                if (status == "Completed")
                    data = data.Where(a => a.OutTime != null);

                if (status == "NotCheckedOut")
                    data = data.Where(a => a.OutTime == null);
            }

            var list = data.OrderBy(a => a.Date).ToList();
            var employees = _context.Employees.ToList();

            using (var pkg = new ExcelPackage())
            {
                var ws = pkg.Workbook.Worksheets.Add("Attendance");

                ws.Cells[1, 1].Value = "Employee Code";
                ws.Cells[1, 2].Value = "Employee Name";
                ws.Cells[1, 3].Value = "Date";
                ws.Cells[1, 4].Value = "Check In";
                ws.Cells[1, 5].Value = "Check Out";
                ws.Cells[1, 6].Value = "Total Hours";
                ws.Cells[1, 7].Value = "Status";

                ws.Row(1).Style.Font.Bold = true;

                int row = 2;

                foreach (var a in list)
                {
                    var emp = employees.FirstOrDefault(e => e.EmployeeCode == a.Emp_Code);

                    ws.Cells[row, 1].Value = a.Emp_Code;
                    ws.Cells[row, 2].Value = emp?.Name ?? "--";
                    ws.Cells[row, 3].Value = a.Date.ToString("dd-MMM-yyyy");
                    ws.Cells[row, 4].Value = a.InTime.HasValue ? a.InTime.Value.ToString(@"hh\:mm") : "--";
                    ws.Cells[row, 5].Value = a.OutTime.HasValue? a.OutTime.Value.ToString(@"hh\:mm") : "--";

                    // ⭐ TOTAL HOURS
                    string totalHours = "--";
                    if (a.InTime.HasValue && a.OutTime.HasValue)
                    {
                        var diff = a.OutTime.Value - a.InTime.Value;
                        totalHours = $"{diff.Hours}h {diff.Minutes}m";
                    }

                    ws.Cells[row, 6].Value = totalHours;
                    ws.Cells[row, 7].Value = a.OutTime == null ? "Not Checked Out" : "Completed";

                    row++;
                }

                ws.Cells.AutoFitColumns();

                return File(
                    pkg.GetAsByteArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "Attendance.xlsx"
                );
            }
        }


        public IActionResult CalendarView()
        {
            var data = _context.AttendanceRecords
                .Select(a => new
                {
                    title = a.Emp_Code + " - " +
                        (!a.InTime.HasValue ? "Absent" :
                        (a.OutTime.HasValue ? "Present" : "Not Checked Out")),
                    start = a.Date.ToString("yyyy-MM-dd"),
                    color = !a.InTime.HasValue ? "#dc3545" :
                            (a.OutTime.HasValue ? "#28a745" : "#ffc107")
                })
                .ToList();

            ViewBag.AttendanceJson = JsonConvert.SerializeObject(data);

            return View();
        }

    }
}
