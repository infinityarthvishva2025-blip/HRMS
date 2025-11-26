using HRMS.Data;
using HRMS.Models;
using HRMS.Services;
using HRMS.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public IActionResult EmployeeSummary(string empCode, DateTime? from, DateTime? to)
        {
            if (empCode == null)
                return BadRequest();

            DateTime start = (from ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)).Date;
            DateTime end = (to ?? start.AddMonths(1).AddDays(-1)).Date;

            var list = _context.Attendances
                .Where(a => a.Emp_Code == empCode && a.Date >= start && a.Date <= end)
                .OrderBy(a => a.Date)
                .ToList();

            return View(list);
        }

        // =========================================================
        // HR LIST PAGE (FILTER + SEARCH)
        // =========================================================
        public IActionResult Index(string search, DateTime? fromDate, DateTime? toDate, string status)
        {
            var query = from a in _context.Attendances
                        join e in _context.Employees
                             on a.Emp_Code equals e.EmployeeCode into empJoin
                        from e in empJoin.DefaultIfEmpty()
                        select new { a, e };

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    x.e != null && (x.e.EmployeeCode.Contains(search) || x.e.Name.Contains(search)));
            }

            if (fromDate.HasValue)
                query = query.Where(x => x.a.Date >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(x => x.a.Date <= toDate.Value);

            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                if (status == "Completed")
                    query = query.Where(x => x.a.InTime != null && x.a.OutTime != null);

                else if (status == "NotCheckedOut")
                    query = query.Where(x => x.a.InTime != null && x.a.OutTime == null);
            }

            var model = query
                .OrderBy(x => x.e.EmployeeCode)
                .ThenBy(x => x.a.Date)
                .AsEnumerable()
                .Select(x => new AttendanceIndexVm
                {
                    Emp_Code = x.e?.EmployeeCode,
                    EmpName = x.e?.Name ?? string.Empty,
                    AttDate = x.a.Date,
                    Status = x.a.Status ?? string.Empty,
                    InTime = x.a.InTime ?? TimeSpan.Zero,
                    OutTime = x.a.OutTime ?? TimeSpan.Zero,
                    IsLate = IsLate(x.a),

                    // ⭐ TOTAL HOURS
                    TotalHours = (x.a.InTime.HasValue && x.a.OutTime.HasValue)
                        ? $"{(x.a.OutTime.Value - x.a.InTime.Value).Hours}h {(x.a.OutTime.Value - x.a.InTime.Value).Minutes}m"
                        : "--"
                })
                .ToList();

            ViewBag.Search = search;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
            ViewBag.Status = status;
            ViewBag.EmployeeList = _context.Employees.ToList();

            return View(model);
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
                    ws.Cells[row, 4].Value = a.InTime?.ToString("hh:mm tt") ?? "--";
                    ws.Cells[row, 5].Value = a.OutTime?.ToString("hh:mm tt") ?? "--";

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

    }
}
