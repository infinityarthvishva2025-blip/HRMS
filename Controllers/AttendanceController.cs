using System;
using System.Linq;
using HRMS.Data;
using HRMS.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

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

            var attendance = _context.Attendances
                .FirstOrDefault(a => a.Emp_Code == empCode && a.Date == today);

            return View(attendance);
        }

        // =========================================================
        // CHECK IN
        // =========================================================
        public IActionResult CheckIn()
        {
            string empCode = HttpContext.Session.GetString("EmpCode");
            if (empCode == null)
                return RedirectToAction("Login", "Account");

            var today = DateTime.Today;

            bool alreadyCheckedIn = _context.Attendances
                .Any(a => a.Emp_Code == empCode && a.Date == today);

            if (!alreadyCheckedIn)
            {
                var rec = new Attendance
                {
                    Emp_Code = empCode,
                    Date = today,
                    Status = "P",
                    InTime = DateTime.Now,
                    OutTime = null,
                    Total_Hours = TimeSpan.Zero
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
            if (empCode == null)
                return RedirectToAction("Login", "Account");

            var today = DateTime.Today;

            var attendance = _context.Attendances
                .FirstOrDefault(a => a.Emp_Code == empCode && a.Date == today);

            if (attendance != null && attendance.OutTime == null)
            {
                attendance.OutTime = DateTime.Now;

                if (attendance.InTime.HasValue)
                {
                    attendance.Total_Hours = attendance.OutTime.Value - attendance.InTime.Value;
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

            if (empCode == null)
                return RedirectToAction("Login", "Account");

            return RedirectToAction(nameof(EmployeeSummary), new { empCode });
        }

        // =========================================================
        // EMPLOYEE SUMMARY PAGE
        // =========================================================
        public IActionResult EmployeeSummary(string empCode, DateTime? fromDate, DateTime? toDate)
        {
            if (empCode == null)
                return BadRequest();

            DateTime start = (fromDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1)).Date;
            DateTime end = (toDate ?? start.AddMonths(1).AddDays(-1)).Date;

            var attendance = _context.Attendances
                .Where(a =>
                    a.Emp_Code == empCode &&
                    a.Date >= start &&
                    a.Date <= end
                )

                .OrderBy(a => a.Date)
                .ToList();

            return View(attendance);
        }

        // =========================================================
        // HR LIST PAGE (FILTER + SEARCH)
        // =========================================================
        public IActionResult Index(string search, DateTime? fromDate, DateTime? toDate)
        {
            // Load employees for name mapping
            ViewBag.EmployeeList = _context.Employees.ToList();

            var data = _context.Attendances.AsQueryable();

            if (fromDate.HasValue)
                data = data.Where(a => a.Date >= fromDate.Value.Date);

            if (toDate.HasValue)
                data = data.Where(a => a.Date <= toDate.Value.Date);

            if (!string.IsNullOrEmpty(search))
            {
                data = data.Where(a =>
                    a.Emp_Code.Contains(search)
                );
            }

            var list = data.OrderByDescending(a => a.Date).ToList();
            return View(list);
        }




        //[HttpGet]
        //public IActionResult ExportFiltered(string search, DateTime? fromDate, DateTime? toDate, string status)
        //{
        //    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        //    var data = _context.Attendances.AsQueryable();

        //    // FILTERS
        //    if (fromDate.HasValue)
        //        data = data.Where(a => a.Date >= fromDate.Value.Date);

        //    if (toDate.HasValue)
        //        data = data.Where(a => a.Date <= toDate.Value.Date);

        //    if (!string.IsNullOrEmpty(search))
        //        data = data.Where(a => a.EmpCode.Contains(search));

        //    if (!string.IsNullOrEmpty(status) && status != "All")
        //    {
        //        if (status == "Completed")
        //            data = data.Where(a => a.OutTime != null);

        //        if (status == "NotCheckedOut")
        //            data = data.Where(a => a.OutTime == null);
        //    }

        //    var list = data.OrderBy(a => a.Date).ToList();

        //    // Get employee names
        //    var employees = _context.Employees.ToList();

        //    using (var pkg = new ExcelPackage())
        //    {
        //        var ws = pkg.Workbook.Worksheets.Add("Attendance");

        //      ws.Cells[1, 1].Value = "Employee Code";
        //        ws.Cells[1, 2].Value = "Date";
        //        ws.Cells[1, 3].Value = "Status";
        //        ws.Cells[1, 4].Value = "In Time";
        //        ws.Cells[1, 5].Value = "Out Time";
        //        ws.Cells[1, 6].Value = "Total Hours";

        //        ws.Row(1).Style.Font.Bold = true;

        //        int row = 2;

        //        foreach (var a in list)
        //        {
        //            var emp = employees.FirstOrDefault(e => e.EmployeeCode == a.EmpCode);

        //            ws.Cells[row, 1].Value = a.EmpCode;
        //            ws.Cells[row, 2].Value = emp?.Name ?? "--";
        //            ws.Cells[row, 3].Value = a.Date.ToString("dd-MMM-yyyy");
        //            ws.Cells[row, 4].Value = a.InTime?.ToString("hh:mm tt") ?? "--";
        //            ws.Cells[row, 5].Value = a.OutTime?.ToString("hh:mm tt") ?? "--";

        //            // Total Hours calculation
        //            string totalHours = "--";
        //            if (a.InTime.HasValue && a.OutTime.HasValue)
        //            {
        //                var diff = a.OutTime.Value - a.InTime.Value;
        //                totalHours = $"{diff.Hours}h {diff.Minutes}m";
        //            }

        //            ws.Cells[row, 6].Value = totalHours;

        //            ws.Cells[row, 7].Value = a.OutTime == null ? "Not Checked Out" : "Completed";

        //            row++;
        //        }

        //        ws.Cells.AutoFitColumns();

        //        // DOWNLOAD
        //        return File(
        //            pkg.GetAsByteArray(),
        //            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        //            "Attendance.xlsx"
        //        );
        //    }
        //}

    }
}
