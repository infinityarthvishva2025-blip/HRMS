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

            // Get employee using Emp_Code
            var employee = _context.Employees
                                   .FirstOrDefault(e => e.EmployeeCode == empCode);

            if (employee == null)
                return RedirectToAction("Login", "Account");

            Attendance att = new Attendance
            {
                Id = employee.Id,                  // ← store employee id manually
                Emp_Code = empCode,
                Date = DateTime.Today,
                Status = "P",

                InTime = DateTime.Now.TimeOfDay,
                Att_Date = DateTime.Now,           // ← store attendance datetime

                OutTime = null,
                Total_Hours = null,
                IsLate = false,
                LateMinutes = 0,
                CorrectionRequested = false,
                CorrectionStatus = "None"
            };

            _context.Attendances.Add(att);
            _context.SaveChanges();

            TempData["CheckedIn"] = true;
            return RedirectToAction("EmployeePanel");
        }



        // =========================================================
        // CHECK OUT
        // =========================================================
        public IActionResult CheckOut()
{
    string empCode = HttpContext.Session.GetString("EmpCode");
    if (empCode == null)
        return RedirectToAction("Login", "Account");

    DateTime today = DateTime.Today;

    var record = _context.Attendances
        .FirstOrDefault(a => a.Emp_Code == empCode && a.Date == today);

    if (record == null)
    {
        TempData["EarlyCheckout"] = "No active attendance found.";
        TempData.Keep();
        return RedirectToAction("EmployeePanel");
    }

    if (record.OutTime != null)
    {
        TempData["CheckoutSuccess"] = $"You already checked out at {record.OutTime.Value}.";
        TempData.Keep();
        return RedirectToAction("EmployeePanel");
    }

    record.OutTime = DateTime.Now.TimeOfDay;
            record.Att_Date = record.Date;

            if (record.InTime != null)
    {
        TimeSpan worked = record.OutTime.Value - record.InTime.Value;
        TimeSpan shift = TimeSpan.FromMinutes(510); // 8.5 hours

        if (worked < shift)
        {
            TimeSpan remaining = shift - worked;

                    TempData["EarlyTime"] = $"{remaining.Hours}h {remaining.Minutes}m";
                    TempData["EarlyCheckout"] = "Early Checkout";
                 
                }
                else
        {
            TimeSpan extra = worked - shift;

            TempData["LateTime"] = $"{extra.Hours}h {extra.Minutes}m";
            TempData["LateCheckout"] = "Great! Overtime";
                    
                }
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

            // Default date range
            if (!from.HasValue)
                from = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            if (!to.HasValue)
                to = DateTime.Now.Date;

            DateTime start = from.Value.Date;
            DateTime end = to.Value.Date;

            // Fetch DB attendance
            var dbAttendance = _context.Attendances
                .Where(a => a.Emp_Code == empCode && a.Date >= start && a.Date <= end)
                .ToList();

            List<AttendanceRecordVm> finalList = new List<AttendanceRecordVm>();

            // Loop through date range to fill missing days
            for (var date = start; date <= end; date = date.AddDays(1))
            {
                var rec = dbAttendance.FirstOrDefault(a => a.Date == date);

                // -------- WEEKLY OFF (Sunday = WO) ----------
                if (date.DayOfWeek == DayOfWeek.Sunday)
                {
                    finalList.Add(new AttendanceRecordVm
                    {
                        Date = date,
                        Status = "WO",          // Weekly Off
                        InTime = null,
                        OutTime = null,
                        CorrectionRequested = false
                    });
                    continue;
                }

                // -------- WEEKLY OFF (Saturday = WOP) ----------
                if (date.DayOfWeek == DayOfWeek.Saturday)
                {
                    finalList.Add(new AttendanceRecordVm
                    {
                        Date = date,
                        Status = "WOP",         // Week Off Present? (Your existing logic)
                        InTime = null,
                        OutTime = null,
                        CorrectionRequested = false
                    });
                    continue;
                }

                // -------- ATTENDANCE EXISTS ----------
                if (rec != null)
                {
                    string finalStatus;

                    if (rec.Status == "L")
                        finalStatus = "L";              // Leave
                    else if (rec.InTime.HasValue && rec.OutTime.HasValue)
                        finalStatus = "P";              // Present
                    else if (rec.InTime.HasValue && !rec.OutTime.HasValue)
                        finalStatus = "AUTO";           // Auto checkout
                    else
                        finalStatus = "A";              // Absent

                    finalList.Add(new AttendanceRecordVm
                    {
                        Date = rec.Date,
                        Status = finalStatus,
                        InTime = rec.InTime,
                        OutTime = rec.OutTime,
                        CorrectionRequested = rec.CorrectionRequested,
                        CorrectionStatus = rec.CorrectionStatus
                    });
                }
                else
                {
                    // -------- NO ATTENDANCE → Absent --------
                    finalList.Add(new AttendanceRecordVm
                    {
                        Date = date,
                        Status = "A",
                        InTime = null,
                        OutTime = null,
                        CorrectionRequested = false
                    });
                }
            }

            // Calculate summary
            var summary = new EmployeeAttendanceSummaryViewModel
            {
                Employee = emp,
                AttendanceRecords = finalList.OrderByDescending(d => d.Date).ToList(),
                FromDate = start,
                ToDate = end,

                TotalDays = finalList.Count,

                AverageWorkingHours =
        finalList
            .Where(a => a.InTime.HasValue && a.OutTime.HasValue)
            .Select(a => (a.OutTime.Value - a.InTime.Value).TotalHours)
            .DefaultIfEmpty(0)
            .Average()
            .ToString("0.0") + " Hrs"
            };


            return View(summary);
        }





        public IActionResult Index(string search, DateTime? fromDate, DateTime? toDate, string status)
        {
            var attendance = _context.Attendances.AsQueryable();

            // ---------------------------------------------
            // 1️⃣ DEFAULT → SHOW ONLY TODAY
            // ---------------------------------------------
            bool isFilterApplied =
                !string.IsNullOrEmpty(search) ||
                fromDate.HasValue ||
                toDate.HasValue ||
                !string.IsNullOrEmpty(status);

            DateTime today = DateTime.Today;

            if (!isFilterApplied)
            {
                fromDate = today;
                toDate = today;
            }

            // ---------------------------------------------
            // 2️⃣ ENSURE SAFE DATE RANGE
            // ---------------------------------------------
            DateTime start = fromDate?.Date ?? today;
            DateTime end = toDate?.Date ?? today;

            attendance = attendance.Where(a => a.Date >= start && a.Date <= end);

            // ---------------------------------------------
            // 3️⃣ Search Filter
            // ---------------------------------------------
            if (!string.IsNullOrEmpty(search))
                attendance = attendance.Where(a => a.Emp_Code.Contains(search));

            // ---------------------------------------------
            // 4️⃣ Status Filter
            // ---------------------------------------------
            if (status == "NotCheckedOut")
                attendance = attendance.Where(a => a.InTime != null && a.OutTime == null);
            else if (status == "Completed")
                attendance = attendance.Where(a => a.InTime != null && a.OutTime != null);

            // ---------------------------------------------
            // 5️⃣ FETCH ONLY DB RECORDS — NO FAKE ROWS
            // ---------------------------------------------
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

                    CorrectionRequested = a.CorrectionRequested,
                    CorrectionStatus = a.CorrectionStatus
                })
                .ToList();

            // ---------------------------------------------
            // 6️⃣ ADD WEEKLY OFF LABELS (ONLY FOR EXISTING RECORDS)
            // ---------------------------------------------
            foreach (var item in list)
            {
                if (item.AttDate.DayOfWeek == DayOfWeek.Sunday)
                    item.Status = "WO";      // Weekly Off (Sunday)
                else if (item.AttDate.DayOfWeek == DayOfWeek.Saturday)
                    item.Status = "WOP";     // Saturday Weekly Off
            }

            // ---------------------------------------------
            // 7️⃣ ViewBag Setup
            // ---------------------------------------------
            ViewBag.FromDate = start.ToString("dd-MM-yyyy");
            ViewBag.ToDate = end.ToString("dd-MM-yyyy");
            ViewBag.Search = search;
            ViewBag.Status = status;

            ViewBag.EmployeeList = _context.Employees.ToList();
            ViewBag.PendingRequests = _context.Attendances
                .Count(a => a.CorrectionRequested == true && a.CorrectionStatus == "Pending");

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



        // ===============================
        // MANUAL ATTENDANCE (HR ENTERS)
        // ===============================

        [HttpGet]
        public IActionResult ManualAttendance()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ManualAttendance(string EmpCode, DateTime Date, TimeSpan InTime, TimeSpan OutTime)
        {
            if (string.IsNullOrEmpty(EmpCode))
            {
                TempData["Error"] = "Employee code is required.";
                return View();
            }

            // Find Employee
            var emp = _context.Employees.FirstOrDefault(e => e.EmployeeCode == EmpCode);

            if (emp == null)
            {
                TempData["Error"] = "Employee does not exist.";
                return View();
            }

            // Check if attendance already exists for that date
            var existing = _context.Attendances
                .FirstOrDefault(a => a.Emp_Code == EmpCode && a.Date == Date.Date);

            if (existing != null)
            {
                TempData["Error"] = "Attendance for this date already exists!";
                return View();
            }

            // Create new attendance record
            Attendance att = new Attendance
            {
                Emp_Code = EmpCode,
                Date = Date.Date,
                InTime = InTime,
                OutTime = OutTime,
                Status = "P"
            };

            // Calculate working hours
            att.Total_Hours = (decimal)(OutTime - InTime).TotalHours;

            _context.Attendances.Add(att);
            _context.SaveChanges();

            TempData["Success"] = "Manual Attendance added successfully!";
            return RedirectToAction("ManualAttendance");
        }

    }
}
        public async Task<IActionResult> EarnCompOff(int empId, DateTime workDate)
        {
            var emp = await _context.Employees.FindAsync(empId);
            if (emp == null) return Json("Employee not found");

            // Earn only on weekly off / holiday
            if (workDate.DayOfWeek != DayOfWeek.Sunday &&
                workDate.DayOfWeek != DayOfWeek.Saturday) // optional
            {
                return Json("Not eligible for Comp-Off");
            }

            emp.CompOffBalance += 1;
            emp.LastCompOffEarnedDate = DateTime.Today;

            await _context.SaveChangesAsync();
            return Json("Comp-Off earned successfully");
        }

    }
}



