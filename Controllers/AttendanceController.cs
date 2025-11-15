using System;
using System.Collections.Generic;
using System.Linq;
using HRMS.Data;
using HRMS.Models;
using HRMS.Models.ViewModels;
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

        // ============================================================
        // EMPLOYEE PANEL (Check-In / Check-Out)
        // ============================================================

        public IActionResult EmployeePanel()
        {
            int? empId = HttpContext.Session.GetInt32("EmployeeId");

            if (empId == null)
                return RedirectToAction("Login", "Account");

            var today = DateTime.Today;

            var attendance = _context.Attendances
                .FirstOrDefault(a => a.EmployeeId == empId && a.CheckInTime.Date == today);

            return View(attendance);
        }

        public IActionResult CheckIn()
        {
            int? empId = HttpContext.Session.GetInt32("EmployeeId");

            if (empId == null)
                return RedirectToAction("Login", "Account");

            var today = DateTime.Today;

            bool alreadyCheckedIn = _context.Attendances
                .Any(a => a.EmployeeId == empId && a.CheckInTime.Date == today);

            if (!alreadyCheckedIn)
            {
                var record = new Attendance
                {
                    EmployeeId = empId.Value,
                    CheckInTime = DateTime.Now
                };

                _context.Attendances.Add(record);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(EmployeePanel));
        }

        public IActionResult CheckOut()
        {
            int? empId = HttpContext.Session.GetInt32("EmployeeId");

            if (empId == null)
                return RedirectToAction("Login", "Account");

            var today = DateTime.Today;

            var attendance = _context.Attendances
                .FirstOrDefault(a => a.EmployeeId == empId && a.CheckInTime.Date == today);

            if (attendance != null && attendance.CheckOutTime == null)
            {
                attendance.CheckOutTime = DateTime.Now;
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(EmployeePanel));
        }

        // ============================================================
        // EMPLOYEE SELF SUMMARY
        // ============================================================

        public IActionResult MySummary()
        {
            int? empId = HttpContext.Session.GetInt32("EmployeeId");

            if (empId == null)
                return RedirectToAction("Login", "Account");

            return RedirectToAction(nameof(EmployeeSummary), new { employeeId = empId.Value });
        }

        // ============================================================
        // EMPLOYEE-WISE SUMMARY
        // ============================================================

        public IActionResult EmployeeSummary(int employeeId, DateTime? fromDate, DateTime? toDate)
        {
            if (employeeId <= 0)
                return BadRequest("Invalid Employee ID");

            var emp = _context.Employees.FirstOrDefault(e => e.Id == employeeId);
            if (emp == null)
                return NotFound();

            DateTime start = fromDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime end = toDate ?? start.AddMonths(1).AddDays(-1);

            var attendance = _context.Attendances
                .Where(a => a.EmployeeId == employeeId &&
                            a.CheckInTime.Date >= start &&
                            a.CheckInTime.Date <= end)
                .OrderBy(a => a.CheckInTime)
                .ToList();

            int lateDays = attendance.Count(a => a.CheckInTime.TimeOfDay > new TimeSpan(10, 0, 0));
            int earlyLeaveDays = attendance.Count(a => a.CheckOutTime.HasValue &&
                                                       a.CheckOutTime.Value.TimeOfDay < new TimeSpan(18, 0, 0));

            double totalHours = attendance
                .Where(a => a.CheckOutTime.HasValue)
                .Sum(a => (a.CheckOutTime.Value - a.CheckInTime).TotalHours);

            int totalDays = attendance.Count;
            double avgHours = totalDays > 0 ? totalHours / totalDays : 0;

            var model = new EmployeeAttendanceSummaryViewModel
            {
                Employee = emp,
                AttendanceRecords = attendance,
                TotalDays = totalDays,
                TotalLateDays = lateDays,
                TotalEarlyLeaveDays = earlyLeaveDays,
                AverageWorkingHours = Math.Round(avgHours, 2),
                FromDate = start,
                ToDate = end
            };

            return View(model);
        }

        // ============================================================
        // HR LIST PAGE — SEARCH + FILTERS + EXPORT
        // ============================================================

        public IActionResult Index(string search, DateTime? fromDate, DateTime? toDate, string status)
        {
            var today = DateTime.Today;

            var attendance = _context.Attendances
                .Include(a => a.Employee)
                .Where(a => a.CheckInTime.Date == today)   // ✅ Only today's records
                .AsQueryable();


            if (!string.IsNullOrEmpty(search))
            {
                attendance = attendance.Where(a =>
                    a.Employee.Name.Contains(search) ||
                    a.Employee.EmployeeCode.Contains(search));
            }

            if (fromDate.HasValue)
                attendance = attendance.Where(a => a.CheckInTime.Date >= fromDate.Value);

            if (toDate.HasValue)
                attendance = attendance.Where(a => a.CheckInTime.Date <= toDate.Value);

            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                if (status == "Completed")
                    attendance = attendance.Where(a => a.CheckOutTime != null);

                if (status == "NotCheckedOut")
                    attendance = attendance.Where(a => a.CheckOutTime == null);
            }

            ViewBag.Search = search;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
            ViewBag.Status = status ?? "All";

            var list = attendance
                .OrderByDescending(a => a.CheckInTime)
                .ToList();

            return View(list);
        }

        // ============================================================
        // EXPORT FILTERED RESULTS TO EXCEL
        // ============================================================

        [HttpGet]
        public IActionResult ExportFiltered(string search, DateTime? fromDate, DateTime? toDate, string status)
        {
            var attendance = _context.Attendances
                .Include(a => a.Employee)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                attendance = attendance.Where(a =>
                    a.Employee.Name.Contains(search) ||
                    a.Employee.EmployeeCode.Contains(search));

            if (fromDate.HasValue)
                attendance = attendance.Where(a => a.CheckInTime.Date >= fromDate.Value);

            if (toDate.HasValue)
                attendance = attendance.Where(a => a.CheckInTime.Date <= toDate.Value);

            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                if (status == "Completed")
                    attendance = attendance.Where(a => a.CheckOutTime != null);

                if (status == "NotCheckedOut")
                    attendance = attendance.Where(a => a.CheckOutTime == null);
            }

            var data = attendance.ToList();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("Filtered Attendance");

                ws.Cells[1, 1].Value = "Employee Code";
                ws.Cells[1, 2].Value = "Employee Name";
                ws.Cells[1, 3].Value = "Date";
                ws.Cells[1, 4].Value = "Check-In Time";
                ws.Cells[1, 5].Value = "Check-Out Time";
                ws.Cells[1, 6].Value = "Working Hours";

                ws.Row(1).Style.Font.Bold = true;
                int row = 2;

                foreach (var a in data)
                {
                    ws.Cells[row, 1].Value = a.Employee.EmployeeCode;
                    ws.Cells[row, 2].Value = a.Employee.Name;
                    ws.Cells[row, 3].Value = a.CheckInTime.ToString("dd-MMM-yyyy");
                    ws.Cells[row, 4].Value = a.CheckInTime.ToString("hh:mm tt");
                    ws.Cells[row, 5].Value = a.CheckOutTime?.ToString("hh:mm tt") ?? "--";

                    if (a.CheckOutTime.HasValue)
                    {
                        var diff = a.CheckOutTime.Value - a.CheckInTime;
                        ws.Cells[row, 6].Value = $"{diff.Hours}h {diff.Minutes}m";
                    }
                    else
                    {
                        ws.Cells[row, 6].Value = "--";
                    }

                    row++;
                }

                ws.Cells.AutoFitColumns();

                return File(package.GetAsByteArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "Filtered_Attendance.xlsx");
            }
        }

    }
}
