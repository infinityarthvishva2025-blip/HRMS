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
using static System.Runtime.InteropServices.JavaScript.JSType;

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
                    Emp_Code = empCode,                // correct property name
                    Date = today,                      // correct date field
                    Status = "P",                      // present
                    InTime = DateTime.Now.TimeOfDay,   // convert to TimeSpan
                    OutTime = null,
                    Total_Hours = 0                    // or TimeSpan.Zero if you change type
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
                // Out time (TimeSpan)
                rec.OutTime = DateTime.Now.TimeOfDay;

                // Total Hours = difference between InTime and OutTime
                if (rec.InTime.HasValue && rec.OutTime.HasValue)
                {
                    TimeSpan diff = rec.OutTime.Value - rec.InTime.Value;
                    rec.Total_Hours = (decimal)diff.TotalHours;
                }

                _context.SaveChanges();
            }

            return RedirectToAction(nameof(EmployeePanel));
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
            // Base query
            var query = from a in _context.Attendances
                        join e in _context.Employees
                             on a.Emp_Code equals e.EmployeeCode into empJoin
                        from e in empJoin.DefaultIfEmpty()
                        select new { a, e };

            // ================================
            // 1️⃣ SEARCH FILTER (Name or Code)
            // ================================
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    (x.e != null && (
                        x.e.EmployeeCode.Contains(search) ||
                        x.e.Name.Contains(search))));
            }

            // ================================
            // 2️⃣ DATE RANGE FILTER
            // ================================
            if (fromDate.HasValue)
            {
                query = query.Where(x => x.a.Date >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(x => x.a.Date <= toDate.Value);
            }

            // ================================
            // 3️⃣ STATUS FILTER
            // ================================
            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                if (status == "Completed")        // In + Out exists
                    query = query.Where(x => x.a.InTime != null && x.a.OutTime != null);

                else if (status == "NotCheckedOut") // In exists but Out is null
                    query = query.Where(x => x.a.InTime != null && x.a.OutTime == null);
            }

            // ================================
            // 4️⃣ BUILD VIEW MODEL
            // ================================
            var model = query
                .OrderBy(x => x.e.EmployeeCode)
                .ThenBy(x => x.a.Date)
                .AsEnumerable()   // safe after filtering
                .Select(x => new AttendanceIndexVm
                {
                    Emp_Code = x.e?.EmployeeCode,
                    EmpName = x.e?.Name ?? string.Empty,
                    AttDate = x.a.Date,
                    Status = x.a.Status ?? string.Empty,
                    InTime = x.a.InTime ?? TimeSpan.Zero,
                    OutTime = x.a.OutTime ?? TimeSpan.Zero,
                    IsLate = IsLate(x.a)
                })
                .ToList();

            // Required to refill dropdowns in view
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

            // Sunday = WO (never late)
            if (day == DayOfWeek.Sunday)
                return false;

            TimeSpan cutoff = (day == DayOfWeek.Saturday)
                              ? new TimeSpan(10, 15, 0)   // Saturday cutoff
                              : new TimeSpan(9, 45, 0);   // Weekday cutoff

            return a.InTime.Value > cutoff;
        }




        [HttpGet]
        public IActionResult ExportFiltered(string search, DateTime? fromDate, DateTime? toDate, string status)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var data = _context.Attendances.AsQueryable();

            // FILTERS
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

            // Get employee names
            var employees = _context.Employees.ToList();

            using (var pkg = new ExcelPackage())
            {
                var ws = pkg.Workbook.Worksheets.Add("Attendance");

                // HEADERS
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

                    // Total Hours calculation
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

                // DOWNLOAD
                return File(
                    pkg.GetAsByteArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "Attendance.xlsx"
                );
            }
        }

    }
}