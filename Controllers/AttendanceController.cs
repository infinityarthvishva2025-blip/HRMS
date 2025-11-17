using System;
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
        // EMPLOYEE PANEL
        // ============================================================

        public IActionResult EmployeePanel()
        {
            int? empId = HttpContext.Session.GetInt32("EmployeeId");
            if (empId == null)
                return RedirectToAction("Login", "Account");

            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var attendance = _context.Attendances
                .FirstOrDefault(a =>
                    a.EmployeeId == empId &&
                    a.CheckInTime >= today &&
                    a.CheckInTime < tomorrow
                );

            return View(attendance);
        }

        // ============================================================
        // CHECK-IN
        // ============================================================

        public IActionResult CheckIn()
        {
            int? empId = HttpContext.Session.GetInt32("EmployeeId");
            if (empId == null)
                return RedirectToAction("Login", "Account");

            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            bool alreadyCheckedIn = _context.Attendances
                .Any(a =>
                    a.EmployeeId == empId &&
                    a.CheckInTime >= today &&
                    a.CheckInTime < tomorrow
                );

            if (!alreadyCheckedIn)
            {
                var rec = new Attendance
                {
                    EmployeeId = empId.Value,
                    CheckInTime = DateTime.Now,
                    CheckoutStatus = null,
                    IsLate = DateTime.Now.TimeOfDay > new TimeSpan(10, 0, 0)
                };

                _context.Attendances.Add(rec);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(EmployeePanel));
        }

        // ============================================================
        // CHECK-OUT
        // ============================================================

        public IActionResult CheckOut()
        {
            int? empId = HttpContext.Session.GetInt32("EmployeeId");
            if (empId == null)
                return RedirectToAction("Login", "Account");

            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var attendance = _context.Attendances
                .FirstOrDefault(a =>
                    a.EmployeeId == empId &&
                    a.CheckInTime >= today &&
                    a.CheckInTime < tomorrow
                );

            if (attendance != null && attendance.CheckOutTime == null)
            {
                attendance.CheckOutTime = DateTime.Now;

                attendance.IsEarlyLeave =
                    attendance.CheckOutTime.Value.TimeOfDay < new TimeSpan(18, 0, 0);

                var diff = attendance.CheckOutTime.Value - attendance.CheckInTime.Value;
                attendance.WorkingHours = diff.TotalHours;

                attendance.CheckoutStatus = "Checked out";

                _context.SaveChanges();
            }


            return RedirectToAction(nameof(EmployeePanel));
        }

        // ============================================================
        // AUTO CHECK-OUT (11:59 PM)
        // ============================================================

        private void AutoCheckoutForgottenEmployees()
        {
            var yesterday = DateTime.Today.AddDays(-1);
            var today = DateTime.Today;

            var pending = _context.Attendances
                .Where(a =>
                    a.CheckInTime >= yesterday &&
                    a.CheckInTime < today &&
                    a.CheckOutTime == null
                )
                .ToList();

            foreach (var rec in pending)
            {
                rec.CheckOutTime = yesterday.AddHours(23).AddMinutes(59);
                rec.CheckoutStatus = "Auto Checked-out";

                var diff = rec.CheckOutTime.Value - rec.CheckInTime.Value;
                rec.WorkingHours = diff.TotalHours;

            }

            if (pending.Count > 0)
                _context.SaveChanges();
        }

        // ============================================================
        // EMPLOYEE SUMMARY
        // ============================================================

        public IActionResult MySummary()
        {
            int? empId = HttpContext.Session.GetInt32("EmployeeId");
            if (empId == null)
                return RedirectToAction("Login", "Account");

            return RedirectToAction(nameof(EmployeeSummary), new { employeeId = empId.Value });
        }

        public IActionResult EmployeeSummary(int employeeId, DateTime? fromDate, DateTime? toDate)
        {
            if (employeeId <= 0)
                return BadRequest();

            var emp = _context.Employees.FirstOrDefault(e => e.Id == employeeId);
            if (emp == null)
                return NotFound();

            DateTime start = (fromDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1)).Date;
            DateTime end = (toDate ?? start.AddMonths(1).AddDays(-1)).Date;

            var attendance = _context.Attendances
                .Where(a =>
                    a.EmployeeId == employeeId &&
                    a.CheckInTime >= start &&
                    a.CheckInTime < end.AddDays(1)
                )
                .OrderBy(a => a.CheckInTime)
                .ToList();

            int lateDays = attendance.Count(a => a.IsLate);
            int earlyLeaves = attendance.Count(a => a.IsEarlyLeave);
            double totalHours = attendance.Sum(a => a.WorkingHours);

            int totalDays = attendance.Count;
            double avgHours = totalDays > 0 ? totalHours / totalDays : 0;

            var model = new EmployeeAttendanceSummaryViewModel
            {
                Employee = emp,
                AttendanceRecords = attendance,
                TotalDays = totalDays,
                TotalLateDays = lateDays,
                TotalEarlyLeaveDays = earlyLeaves,
                AverageWorkingHours = Math.Round(avgHours, 2),
                FromDate = start,
                ToDate = end
            };

            return View(model);
        }

        // ============================================================
        // HR LIST PAGE (FILTER + SEARCH)
        // ============================================================

        public IActionResult Index(string search, DateTime? fromDate, DateTime? toDate, string status)
        {
            AutoCheckoutForgottenEmployees();

            var data = _context.Attendances
                .Include(a => a.Employee)
                .AsQueryable();

            if (fromDate.HasValue)
            {
                var f = fromDate.Value.Date;
                data = data.Where(a => a.CheckInTime >= f);
            }

            if (toDate.HasValue)
            {
                var t = toDate.Value.Date.AddDays(1);
                data = data.Where(a => a.CheckInTime < t);
            }

            if (!string.IsNullOrEmpty(search))
            {
                data = data.Where(a =>
                    a.Employee.Name.Contains(search) ||
                    a.Employee.EmployeeCode.Contains(search));
            }

            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                if (status == "Completed")
                    data = data.Where(a => a.CheckOutTime != null);

                if (status == "NotCheckedOut")
                    data = data.Where(a => a.CheckOutTime == null);
            }

            ViewBag.Search = search;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
            ViewBag.Status = status ?? "All";

            var list = data.OrderByDescending(a => a.CheckInTime).ToList();

            return View(list);
        }

        // ============================================================
        // EXPORT TO EXCEL
        // ============================================================

        [HttpGet]
        public IActionResult ExportFiltered(string search, DateTime? fromDate, DateTime? toDate, string status)
        {
            var data = _context.Attendances
                .Include(a => a.Employee)
                .AsQueryable();

            if (fromDate.HasValue)
            {
                var f = fromDate.Value.Date;
                data = data.Where(a => a.CheckInTime >= f);
            }

            if (toDate.HasValue)
            {
                var t = toDate.Value.Date.AddDays(1);
                data = data.Where(a => a.CheckInTime < t);
            }

            if (!string.IsNullOrEmpty(search))
            {
                data = data.Where(a =>
                    a.Employee.Name.Contains(search) ||
                    a.Employee.EmployeeCode.Contains(search));
            }

            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                if (status == "Completed")
                    data = data.Where(a => a.CheckOutTime != null);

                if (status == "NotCheckedOut")
                    data = data.Where(a => a.CheckOutTime == null);
            }

            var list = data.OrderByDescending(a => a.CheckInTime).ToList();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var pkg = new ExcelPackage())
            {
                var ws = pkg.Workbook.Worksheets.Add("Attendance");

                ws.Cells[1, 1].Value = "Employee Code";
                ws.Cells[1, 2].Value = "Name";
                ws.Cells[1, 3].Value = "Date";
                ws.Cells[1, 4].Value = "Check-In";
                ws.Cells[1, 5].Value = "Check-Out";
                ws.Cells[1, 6].Value = "Hours";
                ws.Cells[1, 7].Value = "Status";

                ws.Row(1).Style.Font.Bold = true;

                int row = 2;

                foreach (var a in list)
                {
                    ws.Cells[row, 1].Value = a.Employee.EmployeeCode;
                    ws.Cells[row, 2].Value = a.Employee.Name;
                    ws.Cells[row, 3].Value = a.CheckInTime?.ToString("dd-MMM-yyyy");
                    ws.Cells[row, 4].Value = a.CheckInTime?.ToString("hh:mm tt");
                    ws.Cells[row, 5].Value = a.CheckOutTime?.ToString("hh:mm tt") ?? "--";
                    ws.Cells[row, 6].Value = Math.Round(a.WorkingHours, 2);
                    ws.Cells[row, 7].Value = a.CheckoutStatus ?? "--";
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
