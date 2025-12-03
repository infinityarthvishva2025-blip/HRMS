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
            if (empCode == null)
                return RedirectToAction("Login", "Account");

            Attendance att = new Attendance
            {
                Emp_Code = empCode,
                Date = DateTime.Now.Date,
                InTime = DateTime.Now.TimeOfDay,
                Status = "Present"
            };

            _context.Attendances.Add(att);
            _context.SaveChanges();

            TempData["CheckedIn"] = true; // Start countdown on UI

            return RedirectToAction("EmployeePanel");
        }

        // =========================================================
        // CHECK OUT
        // =========================================================
        // =========================================================
        // CHECK OUT
        // =========================================================
        public IActionResult CheckOut()
        {
            string empCode = HttpContext.Session.GetString("EmpCode");
            if (empCode == null)
                return RedirectToAction("Login", "Account");

            DateTime today = DateTime.Today;

            // Always fetch TODAY'S record ONLY
            var record = _context.Attendances
                .FirstOrDefault(a => a.Emp_Code == empCode && a.Date == today);

            // No record found
            if (record == null)
            {
                TempData["EarlyCheckout"] = "No active attendance found for today.";
                return RedirectToAction("EmployeePanel");
            }

            // Prevent double checkout
            if (record.OutTime != null)
            {
                TempData["CheckoutSuccess"] = $"You already checked out at {record.OutTime.Value}.";
                return RedirectToAction("EmployeePanel");
            }

            // Set checkout time
            record.OutTime = DateTime.Now.TimeOfDay;

            // If InTime exists calculate hours
            if (record.InTime != null)
            {
                TimeSpan worked = record.OutTime.Value - record.InTime.Value;
                record.Total_Hours = (decimal)worked.TotalHours;

                TimeSpan shift = TimeSpan.FromHours(8);

                if (worked < shift)
                {
                    TimeSpan remaining = shift - worked;
                    TempData["EarlyCheckout"] =
                        $"Early Checkout — remaining {remaining.Hours}h {remaining.Minutes}m.";
                }
                else
                {
                    TempData["CheckoutSuccess"] =
                        $"Shift completed! Total worked {worked.Hours}h {worked.Minutes}m.";
                }
            }
            else
            {
                TempData["CheckoutSuccess"] = "Checked out successfully.";
            }

            _context.SaveChanges();
            return RedirectToAction("EmployeePanel");
        }



        // =========================================================
        // SUMMARY REDIRECT
        // =========================================================
        public IActionResult MySummary()
        {
            string empCode = HttpContext.Session.GetString("EmpCode");

            if (string.IsNullOrEmpty(empCode))
                return RedirectToAction("Login", "Account");

            var employee = _context.Employees.FirstOrDefault(e => e.EmployeeCode == empCode);

            return RedirectToAction(nameof(EmployeeSummary), new { employeeId = employee.Id });
        }

        // =========================================================
        // EMPLOYEE SUMMARY PAGE
        // =========================================================
        [HttpGet]
       public IActionResult EmployeeSummary(int employeeId, DateTime? from = null, DateTime? to = null)


        {
            if (employeeId <= 0)
                return BadRequest("Invalid employee ID.");

            var emp = _context.Employees.FirstOrDefault(e => e.Id == employeeId);
            if (emp == null)
                return NotFound("Employee not found.");

            string empCode = emp.EmployeeCode;

            if (!from.HasValue)
                from = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            if (!to.HasValue)
                to = DateTime.Now.Date;

            DateTime startDate = from.Value.Date;
            DateTime endDate = to.Value.Date;

            var attendanceRecords = _context.Attendances
                .Where(a => a.Emp_Code == empCode)
                .Where(a => a.Date >= startDate && a.Date <= endDate)
                .OrderByDescending(a => a.Date)
                .ToList();

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
        public IActionResult Index(string search, DateTime? fromDate, DateTime? toDate, string status)
        {
            // ❌ WRONG TABLE FIXED HERE
            var attendance = _context.Attendances.AsQueryable();

            if (!fromDate.HasValue && !toDate.HasValue)
            {
                var firstDay = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var lastDay = firstDay.AddMonths(1).AddDays(-1);

                fromDate = firstDay;
                toDate = lastDay;
            }

            attendance = attendance.Where(a => a.Date >= fromDate && a.Date <= toDate);

            if (!string.IsNullOrEmpty(search))
                attendance = attendance.Where(a => a.Emp_Code.Contains(search));

            if (status == "NotCheckedOut")
                attendance = attendance.Where(a => a.InTime != null && a.OutTime == null);
            else if (status == "Completed")
                attendance = attendance.Where(a => a.InTime != null && a.OutTime != null);

            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
            ViewBag.Search = search;
            ViewBag.Status = status;

            var list = attendance
    .OrderByDescending(a => a.Date)
    .Select(a => new AttendanceIndexVm
    {
        Emp_Code = a.Emp_Code,
        AttDate = a.Date,
        InTime = a.InTime,
        OutTime = a.OutTime,
        Status = a.Status,
        TotalHours = (a.InTime != null && a.OutTime != null)
            ? (a.OutTime.Value - a.InTime.Value).ToString(@"hh\:mm")
            : "--",

        // ✅ ADD THESE TWO
        CorrectionRequested = a.CorrectionRequested,
        CorrectionStatus = a.CorrectionStatus
    })
    .ToList();

            ViewBag.EmployeeList = _context.Employees.ToList();
            ViewBag.PendingRequests = _context.Attendances
    .Count(a => a.CorrectionRequested == true
             && a.CorrectionStatus == "Pending");

            return View(list);
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
                    ws.Cells[row, 4].Value = a.InTime?.ToString(@"hh\:mm") ?? "--";
                    ws.Cells[row, 5].Value = a.OutTime?.ToString(@"hh\:mm") ?? "--";

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

        // =========================================================
        // CALENDAR VIEW
        // =========================================================
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


        public IActionResult RequestCorrection(string empCode, string date, int employeeId)
        {
            DateTime parsedDate = DateTime.ParseExact(date, "yyyy-MM-dd", null);

            var att = _context.Attendances
                .FirstOrDefault(a => a.Emp_Code == empCode && a.Date == parsedDate);

            if (att == null)
                return RedirectToAction("EmployeeSummary", new { employeeId });

            ViewBag.EmployeeId = employeeId;

            return View(att);
        }


        [HttpPost]
        public IActionResult RequestCorrection(string empCode, string date, string CorrectionRemark, int employeeId)
        {
            DateTime parsedDate = DateTime.ParseExact(date, "yyyy-MM-dd", null);

            var att = _context.Attendances
                .FirstOrDefault(a => a.Emp_Code == empCode && a.Date == parsedDate);

            if (att == null)
                return RedirectToAction("EmployeeSummary", new { employeeId });

            att.CorrectionRequested = true;
            att.CorrectionRemark = CorrectionRemark;
            att.CorrectionStatus = "Pending";

            _context.SaveChanges();

            return RedirectToAction("EmployeeSummary", new { employeeId });
        }




        public IActionResult ResolveCorrection(string empCode, DateTime date)
        {
            var att = _context.Attendances
                .FirstOrDefault(x => x.Emp_Code == empCode && x.Date == date.Date);

            if (att == null)
                return NotFound("Attendance record not found.");

            return View(att);
        }




        [HttpPost]
        public IActionResult ResolveCorrection(string empCode, DateTime date,
                                       TimeSpan? InTime, TimeSpan? OutTime,
                                       string actionType)
        {
            // find attendance by EmpCode + Date
            var att = _context.Attendances
                .FirstOrDefault(x => x.Emp_Code == empCode && x.Date == date);

            if (att == null)
                return NotFound();

            // find employee
            var emp = _context.Employees.FirstOrDefault(e => e.EmployeeCode == empCode);
            if (emp == null)
                return NotFound();

            string notificationMessage = "";
            string statusText = "";

            if (actionType == "Approve")
            {
                att.InTime = InTime;
                att.OutTime = OutTime;

                if (InTime.HasValue && OutTime.HasValue)
                {
                    var diff = OutTime.Value - InTime.Value;
                    att.Total_Hours = (decimal)diff.TotalHours;
                }

                att.CorrectionStatus = "Approved";
                statusText = "Approved";
                notificationMessage = "Your attendance correction request for "
                                      + date.ToString("dd-MMM-yyyy")
                                      + " has been APPROVED.";
            }
            else
            {
                att.CorrectionStatus = "Rejected";
                statusText = "Rejected";
                notificationMessage = "Your attendance correction request for "
                                      + date.ToString("dd-MMM-yyyy")
                                      + " has been REJECTED.";
            }

            att.CorrectionRequested = false;

            // ------- CREATE ANNOUNCEMENT (EMPLOYEE NOTIFICATION) -------
            Announcement notif = new Announcement()
            {
                Title = "Attendance Correction Update",
                Message = notificationMessage,
                IsGlobal = false,
                TargetEmployees = emp.Id.ToString(),   // send ONLY to that employee
                CreatedOn = DateTime.UtcNow,
                IsUrgent = false
            };

            _context.Announcements.Add(notif);
            _context.SaveChanges();

            return RedirectToAction("CorrectionRequests");
        }






        public IActionResult CorrectionRequests()
        {
            var pending = _context.Attendances
                .Where(a => a.CorrectionRequested == true)
                .OrderByDescending(a => a.Date)
                .ToList();

            return View(pending);
        }



    }
}
