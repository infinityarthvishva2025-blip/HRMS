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
        private readonly ILogger<AttendanceController> _logger;

        public AttendanceController(ApplicationDbContext context, ILogger<AttendanceController> logger)
        {
            _context = context;
            _logger = logger;
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
                Status = "P"
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

        

        public async Task<IActionResult> CalendarView(int? year, int? month, string department = "all", int itemsPerPage = 50)
        {
            try
            {
                var currentDate = DateTime.Now;
                var selectedYear = year ?? currentDate.Year;
                var selectedMonth = month ?? currentDate.Month;

                // Get employees
                var allEmployees = _context.Attendances.AsQueryable();  //await GetEmployeesFromDatabase(department);

                if (allEmployees == null || !allEmployees.Any())
                {
                    _logger.LogWarning($"No employees found for department: {department}");
                    ViewBag.Message = "No employee data found.";
                }

                // Rest of your code...

                //return View(viewModel);
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading calendar view");
                ViewBag.Error = "Error loading calendar data. Please try again.";
                return View(new CalendarViewModel()); // Return empty model
            }
        }
        private async Task<List<Attendance>> GetAttendanceFromDatabase(int year, int month)
        {
            try
            {
                var firstDay = new DateTime(year, month, 1);
                var lastDay = firstDay.AddMonths(1).AddDays(-1);

                var attendance = await _context.Attendances // If table name is different
                    .Where(a => a.Date >= firstDay && a.Date <= lastDay) // If column is AttendanceDate
                    .Select(a => new Attendance
                    {
                        Id = a.Id, // If primary key is different
                        Emp_Code = a.Emp_Code , // Match with your column
                        Date = a.Date,
                        Status = a.Status, // If column is AttendanceStatus
                                                     // Add other properties
                        InTime = a.InTime,
                        OutTime = a.OutTime,
                        Total_Hours = a.Total_Hours
                    })
                    .ToListAsync();

                return attendance;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting attendance data");
                return new List<Attendance>();
            }
        }




        // -------------------------------------------------------------
        // HIGH DENSITY CALENDAR
        // -------------------------------------------------------------
        public IActionResult HighDensityCalendar(
 int year = 0,
 int month = 0,
 string? search = null,
 string? department = "All",
 int page = 1,
 int pageSize = 20)
        {
            if (year == 0) year = DateTime.Now.Year;
            if (month == 0) month = DateTime.Now.Month;
            if (page < 1) page = 1;

            // ---------------- EMPLOYEE FILTER ----------------
            var empQuery = _context.Employees.AsQueryable();

            // ✅ Search by name or code
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                empQuery = empQuery.Where(e =>
                    e.EmployeeCode.Contains(search) ||
                    e.Name.Contains(search));
            }

            // ✅ Department filter
            if (!string.IsNullOrWhiteSpace(department) && department != "All")
            {
                empQuery = empQuery.Where(e => e.Department == department);
            }

            // ✅ NEW — Hide employees who haven’t joined yet
            empQuery = empQuery.Where(e =>
                !e.JoiningDate.HasValue ||
                (e.JoiningDate.Value.Year < year ||
                 (e.JoiningDate.Value.Year == year && e.JoiningDate.Value.Month <= month)));

            int totalEmployees = empQuery.Count();
            int totalPages = (int)Math.Ceiling(totalEmployees / (double)pageSize);
            if (totalPages > 0 && page > totalPages) page = totalPages;

            var employeesPage = empQuery
                .OrderBy(e => e.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // ---------------- ATTENDANCE DATA ----------------
            var empCodes = employeesPage.Select(e => e.EmployeeCode).ToList();

            var attendance = _context.Attendances
                .Where(a => a.Date.Year == year &&
                            a.Date.Month == month &&
                            empCodes.Contains(a.Emp_Code))
                .ToList();

            // ---------------- DEPARTMENT LIST ----------------
            var departments = _context.Employees
                .Where(e => !string.IsNullOrEmpty(e.Department))
                .Select(e => e.Department)
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            // ---------------- VIEWMODEL ----------------
            var vm = new HighDensityCalendarVM
            {
                Year = year,
                Month = month,
                Search = search,
                Department = department,
                Departments = departments,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalEmployees = totalEmployees,
                Employees = employeesPage,
                Attendance = attendance
            };

            return View(vm);
        }


        // -------------------------------------------------------------
        // EXPORT CALENDAR TO EXCEL
        // -------------------------------------------------------------
        [HttpGet]
            public IActionResult ExportCalendar(
                int year,
                int month,
                string? search,
                string? department = "All")
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                var empQuery = _context.Employees.AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.Trim();
                    empQuery = empQuery.Where(e =>
                        e.EmployeeCode.Contains(search) ||
                        e.Name.Contains(search));
                }

                if (!string.IsNullOrWhiteSpace(department) && department != "All")
                {
                    empQuery = empQuery.Where(e => e.Department == department);
                }

                var employees = empQuery.OrderBy(e => e.Name).ToList();
                var empCodes = employees.Select(e => e.EmployeeCode).ToList();

                var attendance = _context.Attendances
                    .Where(a => a.Date.Year == year &&
                                a.Date.Month == month &&
                                empCodes.Contains(a.Emp_Code))
                    .OrderBy(a => a.Date)
                    .ToList();

                using var pkg = new ExcelPackage();
                var ws = pkg.Workbook.Worksheets.Add("Calendar");

                // header row
                ws.Cells[1, 1].Value = "Employee Code";
                ws.Cells[1, 2].Value = "Employee Name";
                ws.Cells[1, 3].Value = "Date";
                ws.Cells[1, 4].Value = "Status";
                ws.Cells[1, 5].Value = "In Time";
                ws.Cells[1, 6].Value = "Out Time";
                ws.Cells[1, 7].Value = "Total Hours";

                int row = 2;
                foreach (var emp in employees)
                {
                    var empAtt = attendance
                        .Where(a => a.Emp_Code == emp.EmployeeCode)
                        .OrderBy(a => a.Date);

                    foreach (var a in empAtt)
                    {
                        ws.Cells[row, 1].Value = emp.EmployeeCode;
                        ws.Cells[row, 2].Value = emp.Name;
                        ws.Cells[row, 3].Value = a.Date.ToString("yyyy-MM-dd");
                        ws.Cells[row, 4].Value = a.Status;
                        ws.Cells[row, 5].Value = a.InTime?.ToString(@"hh\:mm");
                        ws.Cells[row, 6].Value = a.OutTime?.ToString(@"hh\:mm");
                        ws.Cells[row, 7].Value = a.Total_Hours;
                        row++;
                    }
                }

                ws.Cells.AutoFitColumns();

                var fileName = $"Calendar_{year}_{month}.xlsx";
                return File(pkg.GetAsByteArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
        }
    }



