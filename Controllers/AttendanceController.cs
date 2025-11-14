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

        // ================= EMPLOYEE PANEL (Check-In / Check-Out) =================

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

        // ================= EMPLOYEE SELF SUMMARY REDIRECT =================

        public IActionResult MySummary()
        {
            int? empId = HttpContext.Session.GetInt32("EmployeeId");

            if (empId == null)
                return RedirectToAction("Login", "Account");

            return RedirectToAction(nameof(EmployeeSummary), new { employeeId = empId.Value });
        }

        // ================= EMPLOYEE-WISE SUMMARY (HR + Employee) =================

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

            double totalHours = 0;
            foreach (var a in attendance)
            {
                if (a.CheckOutTime.HasValue)
                {
                    totalHours += (a.CheckOutTime.Value - a.CheckInTime).TotalHours;
                }
            }

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

        // ================= HR LIST VIEW (WITH SUMMARY BUTTONS) =================

        public IActionResult Index()
        {
            var records = _context.Attendances
                .Include(a => a.Employee)
                .OrderByDescending(a => a.CheckInTime)
                .ToList();

            return View(records);
        }

        // ================= MONTHLY EXCEL EXPORT =================

        public IActionResult ExportMonthlyReport(int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);

            var data = _context.Attendances
                .Include(a => a.Employee)
                .Where(a => a.CheckInTime >= startDate && a.CheckInTime < endDate)
                .OrderBy(a => a.Employee.EmployeeCode)
                .ThenBy(a => a.CheckInTime)
                .ToList();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("Monthly Attendance");

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

                var bytes = package.GetAsByteArray();
                string filename = $"Attendance_{month}_{year}.xlsx";

                return File(
                    bytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    filename
                );
            }
        }
    }
}
