using HRMS.Data;
using HRMS.Models;
using HRMS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Controllers
{
    public class LeaveController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWorkflowService _workflow;

        public LeaveController(ApplicationDbContext context, IWorkflowService workflow)
        {
            _context = context;
            _workflow = workflow;
        }

        // -------------------- GET CURRENT EMPLOYEE --------------------
        private int? GetCurrentEmployeeId()
        {
            return HttpContext.Session.GetInt32("EmployeeId");
        }

        // -------------------- APPLY LEAVE PAGE --------------------
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var empId = GetCurrentEmployeeId();
            if (empId == null)
                return RedirectToAction("Login", "Account");

            var emp = await _context.Employees.FindAsync(empId);

            ViewBag.CompOffBalance = emp.CompOffBalance;   // <-- Send balance to view
                                                           

            // ✅ SAFE string cast
            ViewBag.UserRole = emp.Role?.ToString();
            var model = new Leave
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today,
                TotalDays = 1
            };

            return View(model);
        }

        // -------------------- VALIDATE COMP-OFF (BALANCE + EXPIRY) --------------------
        private bool ValidateCompOff(Employee emp, int requiredDays)
        {
            // Balance check
            if (emp.CompOffBalance < requiredDays)
                return false;

            // 30-day expiry
            if (emp.LastCompOffEarnedDate.HasValue)
            {
                DateTime expiry = emp.LastCompOffEarnedDate.Value.AddDays(30);
                if (expiry < DateTime.Today)
                {
                    return false;
                }
            }

            return true;
        }

        // -------------------- CREATE LEAVE (POST) --------------------
        [HttpPost]
        public async Task<IActionResult> Create(Leave model)
        {
            var empId = GetCurrentEmployeeId();
            if (empId == null)
                return RedirectToAction("Login", "Account");

            var employee = await _context.Employees.FindAsync(empId);

            // Fix date range
            model.StartDate = model.StartDate.Date;
            if (!model.EndDate.HasValue || model.EndDate < model.StartDate)
                model.EndDate = model.StartDate;

            model.TotalDays = (model.EndDate.Value - model.StartDate).Days + 1;

            // -------------------- COMPOFF PRE-VALIDATION --------------------
            if (model.LeaveType == "coff")
            {
               // bool valid = ValidateCompOff(employee, model.TotalDays);
                bool valid = ValidateCompOff(employee, (int)model.TotalDays);
                if (!valid)
                {
                    TempData["Error"] = "❌ Not enough Comp-Off balance or Comp-Off expired (valid 30 days).";
                    ViewBag.CompOffBalance = employee.CompOffBalance;
                    return View(model);
                }
            }

            // -------------------- DEFAULT APPROVAL STATUS --------------------
            model.EmployeeId = empId.Value;
            model.CreatedOn = DateTime.Now;
            model.ManagerStatus = "Pending";
            model.HrStatus = "Pending";
            model.VpStatus = "Pending";
            if(employee.Role == "Employee")
            {
                model.DirectorStatus = "-";
            }else
                model.DirectorStatus = "Pending";

            model.OverallStatus = "Pending";

            // -------------------- ROLE-BASED APPROVAL FLOW --------------------
            SetApprovalFlow(employee, model);

            _context.Leaves.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("MyLeaves");
        }

        // -------------------- SET APPROVAL FLOW BASED ON ROLE --------------------
        private void SetApprovalFlow(Employee emp, Leave model)
        {
            switch (emp.Role)
            {
                case "Employee":
                    model.CurrentApproverRole = "Employee";
                    model.NextApproverRole = "HR";
                    break;

                case "HR":
                    model.ManagerStatus = "Approved";
                    model.CurrentApproverRole = "HR";
                    model.NextApproverRole = "GM";
                    break;

                case "GM":
                    model.ManagerStatus = "Approved";
                    model.HrStatus = "Approved";
                    model.CurrentApproverRole = "GM";
                    model.NextApproverRole = "Director";
                    break;

                case "VP":
                    model.ManagerStatus = "Approved";
                    model.HrStatus = "Approved";
                    model.VpStatus = "Approved";
                    model.CurrentApproverRole = "VP";
                    model.NextApproverRole = "Director";
                    break;

                case "Director":
                    model.ManagerStatus = "Approved";
                    model.HrStatus = "Approved";
                    model.VpStatus = "Approved";
                    model.DirectorStatus = "Approved";
                    model.OverallStatus = "Approved";
                    model.CurrentApproverRole = "Director";
                    model.NextApproverRole = "Completed";
                    break;
            }
        }

        // -------------------- GET MY LEAVES --------------------
        public async Task<IActionResult> MyLeaves()
        {
            var empId = GetCurrentEmployeeId();
            var leaves = await _context.Leaves
                .Where(x => x.EmployeeId == empId)
                .OrderByDescending(x => x.CreatedOn)
                .ToListAsync();

            return View(leaves);
        }

        // -------------------- PROCESS APPROVAL --------------------
        private async Task ProcessApproval(int id, string role, bool approve, string remark)
        {
            var leave = await _context.Leaves.FindAsync(id);
            var emp = await _context.Employees.FindAsync(leave.EmployeeId);

            // Update status
            switch (role)
            {
                case "GM":
                    leave.ManagerStatus = approve ? "Approved" : "Rejected";
                    break;
                case "HR":
                    leave.HrStatus = approve ? "Approved" : "Rejected";
                    break;
                case "VP":
                    leave.VpStatus = approve ? "Approved" : "Rejected";
                    break;
                case "Director":
                    leave.DirectorStatus = approve ? "Approved" : "Rejected";
                    break;
            }

            // If rejected
            if (!approve)
            {
                leave.OverallStatus = "Rejected";
                await DeleteLeaveFromAttendance(leave);
                await _context.SaveChangesAsync();
                return;
            }

            // Move workflow
            leave.CurrentApproverRole = role;
            leave.NextApproverRole = await _workflow.GetNextApproverRoleAsync(role);

            // Final approval
            if (leave.NextApproverRole == "Completed")
            {
                leave.OverallStatus = "Approved";

                // Deduct Comp-Off balance
                if (leave.LeaveType == "coff")
                {
                    emp.CompOffBalance -= leave.TotalDays;
                   //emp.CompOffBalance -= (float)leave.TotalDays;
                }

                await SyncLeaveToAttendance(leave);
            }

            await _context.SaveChangesAsync();
        }

        // -------------------- UPDATE ATTENDANCE FOR APPROVED LEAVE --------------------
        private async Task SyncLeaveToAttendance(Leave leave)
        {
            var emp = await _context.Employees.FindAsync(leave.EmployeeId);
            string empCode = emp.EmployeeCode;

            DateTime start = leave.StartDate;
            DateTime end = leave.EndDate.Value;

            for (var d = start; d <= end; d = d.AddDays(1))
                await MarkLeave(empCode, d);

            // Sandwich Rule
            if (start.AddDays(-1).DayOfWeek == DayOfWeek.Sunday)
                await MarkLeave(empCode, start.AddDays(-1));

            if (end.AddDays(1).DayOfWeek == DayOfWeek.Sunday)
                await MarkLeave(empCode, end.AddDays(1));

            await _context.SaveChangesAsync();
        }

        private async Task MarkLeave(string empCode, DateTime date)
        {
            var row = await _context.Attendances
                .FirstOrDefaultAsync(a => a.Emp_Code == empCode && a.Date == date);

            if (row == null)
            {
                _context.Attendances.Add(new Attendance
                {
                    Emp_Code = empCode,
                    Date = date,
                    Status = "L"
                });
            }
            else
            {
                row.Status = "L";
            }
        }

        private async Task DeleteLeaveFromAttendance(Leave leave)
        {
            var emp = await _context.Employees.FindAsync(leave.EmployeeId);

            var items = _context.Attendances
                .Where(a => a.Emp_Code == emp.EmployeeCode &&
                            a.Date >= leave.StartDate &&
                            a.Date <= leave.EndDate);

            _context.Attendances.RemoveRange(items);
        }

        // -------------------- APPROVAL ENDPOINTS --------------------
        public async Task<IActionResult> ManagerApprove(int id, bool approve, string remark)
        {
            await ProcessApproval(id, "GM", approve, remark);
            return RedirectToAction("ManagerApprovalList");
        }

        public async Task<IActionResult> HrApprove(int id, bool approve, string remark)
        {
            await ProcessApproval(id, "HR", approve, remark);
            return RedirectToAction("HrApprovalList");
        }

        public async Task<IActionResult> VpApprove(int id, bool approve, string remark)
        {
            await ProcessApproval(id, "VP", approve, remark);
            return RedirectToAction("VpApprovalList");
        }

        public async Task<IActionResult> DirectorApprove(int id, bool approve, string remark)
        {
            await ProcessApproval(id, "Director", approve, remark);
            return RedirectToAction("DirectorApprovalList");
        }
        public async Task<IActionResult> LeaveReport()
        {
            var now = DateTime.Today;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);

            var empId = GetCurrentEmployeeId();
            if (empId == null)
                return RedirectToAction("Login", "Account");

            var pendingHr = await _context.Leaves.CountAsync(l =>
                l.EmployeeId == empId.Value && l.HrStatus == "Pending");

            var pendingManager = await _context.Leaves.CountAsync(l =>
                l.EmployeeId == empId.Value &&
                l.HrStatus == "Approved" &&
                l.ManagerStatus == "Pending");

            var pendingDirector = await _context.Leaves.CountAsync(l =>
                l.EmployeeId == empId.Value &&
                l.HrStatus == "Approved" &&
                l.ManagerStatus == "Approved" &&
                l.DirectorStatus == "Pending");

            var totalThisMonth = await _context.Leaves.CountAsync(l =>
                l.EmployeeId == empId.Value &&
                l.StartDate >= startOfMonth &&
                l.StartDate <= now);

            ViewBag.PendingManager = pendingManager;
            ViewBag.PendingHr = pendingHr;
            ViewBag.PendingDirector = pendingDirector;
            ViewBag.TotalThisMonth = totalThisMonth;

            var leaves = await _context.Leaves
                .Where(l => l.EmployeeId == empId.Value)
                .Include(l => l.Employee)             // ⭐ REQUIRED
                .OrderByDescending(l => l.CreatedOn)
                .ToListAsync();

            return View(leaves);
        }

        public async Task<IActionResult> ApprovalDashboard()
        {
            var empId = GetCurrentEmployeeId();
            if (empId == null)
                return RedirectToAction("Login", "Account");

            var employee = await _context.Employees.FindAsync(empId);

            if (employee == null)
                return RedirectToAction("Login", "Account");

           // Decide which approval list to show based on role
            return employee.Role switch
            {

                "HR" => RedirectToAction("HrApprovalList"),
                "GM" => RedirectToAction("ManagerApprovalList"),
                "VP" => RedirectToAction("VpApprovalList"),
                "Director" => RedirectToAction("DirectorApprovalList"),
                _ => RedirectToAction("LeaveReport") // Employee → No approval dashboard
            };
        }

        [HttpGet]
        public async Task<IActionResult> GetPendingCount()
        {
            var empId = GetCurrentEmployeeId();
            if (empId == null) return Json(0);

            var emp = await _context.Employees.FindAsync(empId);

            int count = emp.Role switch
            {
                "HR" => await _context.Leaves.CountAsync(l => l.HrStatus == "Pending" && l.CurrentApproverRole == emp.Role),
                "GM" => await _context.Leaves.CountAsync(l => l.ManagerStatus == "Pending" && l.HrStatus == "Approved"),
                "VP" => await _context.Leaves.CountAsync(l => l.ManagerStatus == "Approved" && l.HrStatus == "Approved" && l.VpStatus == "Pending"),
                "Director" => await _context.Leaves.CountAsync(l =>
                                l.ManagerStatus == "Approved" &&
                                l.HrStatus == "Approved" &&
                                l.VpStatus == "Approved" &&
                                l.DirectorStatus == "Pending"),
                _ => 0
            };

            return Json(count);
        }
        public async Task<IActionResult> Dashboard()
        {
            var empId = HttpContext.Session.GetInt32("EmployeeId");
            var emp = await _context.Employees.FindAsync(empId);

            //Pass user role to view
            ViewBag.UserRole = emp.Role;

           // Pass pending counts
            ViewBag.ManagerPending = await _context.Leaves.CountAsync(l => l.ManagerStatus == "Pending");
            ViewBag.HrPending = await _context.Leaves.CountAsync(l => l.ManagerStatus == "Approved" && l.HrStatus == "Pending");
            ViewBag.VpPending = await _context.Leaves.CountAsync(l => l.ManagerStatus == "Approved" && l.HrStatus == "Approved" && l.VpStatus == "Pending");
            ViewBag.DirectorPending = await _context.Leaves.CountAsync(l =>
                                l.ManagerStatus == "Approved" &&
                                l.HrStatus == "Approved" &&
                                l.VpStatus == "Approved" &&
                                l.DirectorStatus == "Pending");

            return View();
        }

        // -------------------- MANAGER APPROVAL --------------------
        //  [AuthorizeRole("Manager")]
        public async Task<IActionResult> ManagerApprovalList()
        {
            // You can filter by manager's team here if you store ManagerId
            var pending = await _context.Leaves
                .Include(l => l.Employee)
                 .Where(l => l.HrStatus == "Approved" &&
                            l.ManagerStatus == "Pending")

                .OrderByDescending(l => l.CreatedOn)
                .OrderBy(l => l.CreatedOn)
                .ToListAsync();

            ViewData["ApproverRole"] = "GM";
            ViewData["ApproveAction"] = "ManagerApprove";

            return View("ApprovalList", pending);
        }


        public async Task<IActionResult> HrApprovalList()
        {
            var pending = await _context.Leaves
                .Include(l => l.Employee)
               .Where(l => l.HrStatus == "Pending")
                .OrderBy(l => l.CreatedOn)
                 .OrderByDescending(l => l.CreatedOn)
                .ToListAsync();

            ViewData["ApproverRole"] = "HR";
            ViewData["ApproveAction"] = "HrApprove";

            return View("ApprovalList", pending);
        }

        // -------------------- VICE PRESIDENT APPROVAL --------------------
        public async Task<IActionResult> VpApprovalList()
        {
            var pending = await _context.Leaves
                .Include(l => l.Employee)
                .Where(l => l.ManagerStatus == "Approved" &&
                            l.HrStatus == "Approved" &&
                            l.VpStatus == "Pending")
                .OrderByDescending(l => l.CreatedOn)
                .ToListAsync();

            ViewData["ApproverRole"] = "VP";
            ViewData["ApproveAction"] = "VpApprove";

            return View("ApprovalList", pending);
        }

        public async Task<IActionResult> DirectorApprovalList()
        {
            var pending = await _context.Leaves
                .Include(l => l.Employee)
                .Where(l => l.ManagerStatus == "Approved" &&
                            l.HrStatus == "Approved" &&
                             l.VpStatus == "Approved" &&
                            l.DirectorStatus == "Pending")
                .OrderBy(l => l.CreatedOn)
                .OrderByDescending(l => l.CreatedOn)
                .ToListAsync();

            ViewData["ApproverRole"] = "Director";
            ViewData["ApproveAction"] = "DirectorApprove";

            return View("ApprovalList", pending);
        }



        [HttpGet]
        public async Task<IActionResult> GetLeaveStatusSummary()
        {
            var data = await _context.Leaves
                .GroupBy(l => l.OverallStatus)
                .Select(g => new
                {
                    status = g.Key,
                    count = g.Count()
                })
                .ToListAsync();

            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetApprovalTableAjax(
    string role,
    DateTime? from,
    DateTime? to,
    string? category,
    string? status,
    string? employee)
        {
            var query = _context.Leaves
                .Include(l => l.Employee)
                .AsQueryable();

            // Role-based base filter
            switch (role)
            {
                case "HR":
                    // show all for manager (you can restrict to team if you have ManagerId)
                    break;

                case "GM":
                    query = query.Where(l => l.ManagerStatus == "Approved");
                    break;

                case "Director":
                    query = query.Where(l => l.ManagerStatus == "Approved" &&
                                             l.HrStatus == "Approved");
                    break;
            }

            if (from.HasValue)
                query = query.Where(l => l.StartDate >= from.Value);

            if (to.HasValue)
                query = query.Where(l => l.StartDate <= to.Value);

            if (!string.IsNullOrWhiteSpace(employee))
                query = query.Where(l => l.Employee != null && l.Employee.Name.Contains(employee));

            if (!string.IsNullOrWhiteSpace(category) &&
                Enum.TryParse<LeaveCategory>(category, out var catEnum))
            {
                query = query.Where(l => l.Category == catEnum);
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
                query = query.Where(l => l.OverallStatus == status);

            var list = await query
                .OrderByDescending(l => l.CreatedOn)
                .ToListAsync();

            // need ApproveAction in partial too
            ViewData["ApproveAction"] = role switch
            {
                "HR" => "HrApprove",
                "Director" => "DirectorApprove",
                _ => "ManagerApprove"
            };
            return View(list);
            //return PartialView("_ApprovalTablePartial", list);
        }
        public async Task<IActionResult> OverallLeaveReport()
        {
            var now = DateTime.Today;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);

            var pendingHr = await _context.Leaves.CountAsync(l => l.HrStatus == "Pending");
            var pendingManager = await _context.Leaves.CountAsync(l =>
                l.HrStatus == "Approved" && l.ManagerStatus == "Pending");

            var pendingDirector = await _context.Leaves.CountAsync(l =>
                l.HrStatus == "Approved" && l.ManagerStatus == "Approved" &&
               l.VpStatus == "Approved" && l.DirectorStatus == "Pending");

            var VPPending= await _context.Leaves.CountAsync(l =>
                l.HrStatus == "Approved" && l.ManagerStatus == "Approved" &&
                l.VpStatus == "Pending");
            var totalThisMonth = await _context.Leaves.CountAsync(l =>
                l.StartDate >= startOfMonth && l.StartDate <= now);

            ViewBag.PendingManager = pendingManager;
            ViewBag.PendingHr = pendingHr;
            ViewBag.PendingDirector = pendingDirector;
            ViewBag.TotalThisMonth = totalThisMonth;

            var leaves = await _context.Leaves
                //.Where(l =>
                //    l.HrStatus == "Pending" ||
                //    l.ManagerStatus == "Pending" ||
                //    l.DirectorStatus == "Pending")
                .Include(l => l.Employee)      // ⭐ REQUIRED
                .OrderByDescending(l => l.CreatedOn)
                .ToListAsync();

            return View(leaves);
        }

        public async Task<IActionResult> GetMonthlyLeaveSummaryAll(int? year)
        {
            int targetYear = year ?? DateTime.Today.Year;

            var raw = await _context.Leaves
                .Where(l => l.StartDate.Year == targetYear)
                .GroupBy(l => l.StartDate.Month)
                .Select(g => new
                {
                    month = g.Key,
                    count = g.Count()
                })
                .ToListAsync();

            // Fill all 12 months
            var result = Enumerable.Range(1, 12)
                .Select(m => new
                {
                    month = m,
                    count = raw.FirstOrDefault(r => r.month == m)?.count ?? 0
                });

            return Json(result);
        }
        public async Task<IActionResult> GetPendingCountsall()
        {
            var hr = await _context.Leaves.CountAsync(l => l.HrStatus == "Pending");

            var manager = await _context.Leaves.CountAsync(l =>
               l.HrStatus == "Approved" &&
                l.ManagerStatus == "Pending");

            var director = await _context.Leaves.CountAsync(l =>
            l.HrStatus == "Approved" &&
            l.ManagerStatus == "Approved" &&
                l.DirectorStatus == "Pending");

            return Json(new
            {
                managerPending = manager,
                hrPending = hr,
                directorPending = director
            });
        }
        [HttpGet]
        public async Task<IActionResult> GetPendingCounts()
        {
            var empId = GetCurrentEmployeeId();
            if (empId == null)
                return Json(new { error = "Not logged in" });

            var hr = await _context.Leaves.CountAsync(l =>
                l.EmployeeId == empId.Value &&           // ⭐ ADDED
                l.HrStatus == "Pending");

            var manager = await _context.Leaves.CountAsync(l =>
                l.EmployeeId == empId.Value &&           // ⭐ ADDED
                l.HrStatus == "Approved" &&
                l.ManagerStatus == "Pending");

            var director = await _context.Leaves.CountAsync(l =>
                l.EmployeeId == empId.Value &&           // ⭐ ADDED
                l.HrStatus == "Approved" &&
                l.ManagerStatus == "Approved" &&
                l.DirectorStatus == "Pending");

            return Json(new
            {
                managerPending = manager,
                hrPending = hr,
                directorPending = director
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetMonthlyLeaveSummary(int? year)
        {
            var empId = GetCurrentEmployeeId();
            if (empId == null)
                return Json(new { error = "Not logged in" });

            int targetYear = year ?? DateTime.Today.Year;

            var raw = await _context.Leaves
                .Where(l => l.EmployeeId == empId.Value &&           // ⭐ ADDED
                            l.StartDate.Year == targetYear)
                .GroupBy(l => l.StartDate.Month)
                .Select(g => new
                {
                    month = g.Key,
                    count = g.Count()
                })
                .ToListAsync();

            // Fill all 12 months
            var result = Enumerable.Range(1, 12)
                .Select(m => new
                {
                    month = m,
                    count = raw.FirstOrDefault(r => r.month == m)?.count ?? 0
                });

            return Json(result);
        }

    }
}

//using HRMS.Data;
//using HRMS.Models;
//using HRMS.Services;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace HRMS.Controllers
//{
//    public class LeaveController : Controller
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly IWorkflowService _workflow;

//        public LeaveController(ApplicationDbContext context, IWorkflowService workflow)
//        {
//            _context = context;
//            _workflow = workflow;
//        }

//        private int? GetCurrentEmployeeId()
//        {
//            return HttpContext.Session.GetInt32("EmployeeId");
//        }

//        // ---------------------------------------
//        // APPLY LEAVE PAGE
//        // ---------------------------------------
//        [HttpGet]
//        public async Task<IActionResult> Create()
//        {
//            var empId = GetCurrentEmployeeId();
//            if (empId == null) return RedirectToAction("Login", "Account");

//            var employee = await _context.Employees.FindAsync(empId);

//            var model = new Leave
//            {
//                StartDate = DateTime.Today,
//                EndDate = DateTime.Today,
//                Employee = employee,
//                TotalDays = 1
//            };

//            return View(model);
//        }


//        // ---------------------------------------
//        // CREATE LEAVE POST
//        // ---------------------------------------
//        [HttpPost]
//        public async Task<IActionResult> Create(Leave model)
//        {
//            var empId = GetCurrentEmployeeId();
//            if (empId == null)
//                return RedirectToAction("Login", "Account");

//            var emp = await _context.Employees.FindAsync(empId);

//            model.EmployeeId = empId.Value;
//            model.StartDate = model.StartDate.Date;
//            if (model.EndDate.HasValue)
//                model.EndDate = model.EndDate.Value.Date;

//            // -------- NORMALIZE DATE RANGE --------
//            if (!model.EndDate.HasValue || model.EndDate < model.StartDate)
//                model.EndDate = model.StartDate;

//            model.TotalDays = (model.EndDate.Value - model.StartDate.Value).TotalDays + 1;

//            // -------- COMPOFF VALIDITY CHECK --------
//            if (model.LeaveType == "coff")
//            {
//                bool isValid = ValidateCompOffBalance(emp, model.TotalDays);

//                if (!isValid)
//                {
//                    TempData["Error"] = "Insufficient Comp-Off balance or Comp-Off expired (valid for 30 days only).";
//                    return View(model);
//                }
//            }

//            // -------- SET APPROVAL FLOW --------
//            model.CreatedOn = DateTime.Now;
//            model.ManagerStatus = "Pending";
//            model.HrStatus = "Pending";
//            model.VpStatus = "Pending";
//            model.DirectorStatus = "Pending";
//            model.OverallStatus = "Pending";

//            SetRoleWiseApprovalFlow(emp, model);

//            _context.Leaves.Add(model);
//            await _context.SaveChangesAsync();

//            return RedirectToAction("MyLeaves");
//        }

//        // ---------------------------------------
//        // VALIDATE VALIDITY & BALANCE (30 DAYS)
//        // ---------------------------------------
//        //private bool ValidateCompOffBalance(Employee emp, double requiredDays)
//        //{
//        //    if (emp.CompOffBalance < requiredDays)
//        //        return false;

//        //    // Check validity → comp-off expires after 30 days
//        //    if (emp.LastCompOffEarnedDate.HasValue)
//        //    {
//        //        if (emp.LastCompOffEarnedDate.Value.AddDays(30) < DateTime.Today)
//        //        {
//        //            emp.CompOffBalance = 0; // expire all
//        //            _context.SaveChanges();
//        //            return false;
//        //        }
//        //    }

//        //    return true;
//        //}
//        private bool ValidateCompOffBalance(Employee emp, double requiredDays)
//        {
//            if (emp.CompOffBalance < requiredDays)
//                return false;

//            // Check validity → comp-off expires after 30 days
//            if (emp.LastCompOffEarnedDate.HasValue)
//            {
//                if (emp.LastCompOffEarnedDate.Value.AddDays(30) < DateTime.Today)
//                {
//                    return false;
//                }
//            }

//            return true;
//        }
//        // ---------------------------------------
//        // SET APPROVAL WORKFLOW
//        // ---------------------------------------
//        private void SetRoleWiseApprovalFlow(Employee emp, Leave model)
//        {
//            switch (emp.Role)
//            {
//                case "Employee":
//                    model.CurrentApproverRole = "Employee";
//                    model.NextApproverRole = "HR";
//                    break;

//                case "HR":
//                    model.ManagerStatus = "Pending";
//                    model.HrStatus = "Pending";
//                    model.CurrentApproverRole = "HR";
//                    model.NextApproverRole = "GM";
//                    break;

//                case "GM":
//                    model.ManagerStatus = "Approved";
//                    model.HrStatus = "Approved";
//                    model.VpStatus = "Approved";
//                    model.CurrentApproverRole = "GM";
//                    model.NextApproverRole = "Director";
//                    break;

//                case "VP":
//                    model.ManagerStatus = "Approved";
//                    model.HrStatus = "Approved";
//                    model.CurrentApproverRole = "VP";
//                    model.NextApproverRole = "Director";
//                    break;

//                case "Director":
//                    model.ManagerStatus = "Approved";
//                    model.HrStatus = "Approved";
//                    model.VpStatus = "Approved";
//                    model.DirectorStatus = "Approved";
//                    model.OverallStatus = "Approved";
//                    model.CurrentApproverRole = "Director";
//                    model.NextApproverRole = "Completed";
//                    break;
//            }
//        }

//        // ---------------------------------------
//        // MANAGER / HR / VP / DIRECTOR APPROVE
//        // ---------------------------------------
//        private async Task ProcessApproval(int id, string role, bool approve, string remark)
//        {
//            var leave = await _context.Leaves.FirstOrDefaultAsync(l => l.Id == id);
//            if (leave == null) return;

//            var emp = await _context.Employees.FindAsync(leave.EmployeeId);

//            // UPDATE STATUS
//            switch (role)
//            {
//                case "Manager": leave.ManagerStatus = approve ? "Approved" : "Rejected"; break;
//                case "HR": leave.HrStatus = approve ? "Approved" : "Rejected"; break;
//                case "VP": leave.VpStatus = approve ? "Approved" : "Rejected"; break;
//                case "Director": leave.DirectorStatus = approve ? "Approved" : "Rejected"; break;
//            }

//            // REJECTED → remove attendance
//            if (!approve)
//            {
//                leave.OverallStatus = "Rejected";
//                await DeleteLeaveFromAttendance(leave);
//                await _context.SaveChangesAsync();
//                return;
//            }

//            // APPROVED → move forward
//            leave.CurrentApproverRole = role;
//            leave.NextApproverRole = await _workflow.GetNextApproverRoleAsync(role);

//            // FINAL APPROVAL
//            if (leave.NextApproverRole == "Completed")
//            {
//                leave.OverallStatus = "Approved";

//                // DEDUCT COMPOFF BALANCE  
//                if (leave.LeaveType == "coff")
//                {
//                    emp.CompOffBalance -= leave.TotalDays;
//                }

//                await SyncLeaveToAttendance(leave);
//            }

//            await _context.SaveChangesAsync();
//        }

//        // ---------------------------------------
//        // SANDWICH + ATTENDANCE LOGIC
//        // ---------------------------------------
//        private async Task SyncLeaveToAttendance(Leave leave)
//        {
//            var emp = await _context.Employees.FindAsync(leave.EmployeeId);
//            string empCode = emp.EmployeeCode;

//            DateTime start = leave.StartDate;
//            DateTime end = leave.EndDate ?? leave.StartDate;

//            // APPLY MAIN DAYS
//            for (var dt = start; dt <= end; dt = dt.AddDays(1))
//                await MarkLeave(empCode, dt);

//            // SANDWICH BEFORE
//            DateTime before = start.AddDays(-1);
//            if (before.DayOfWeek == DayOfWeek.Sunday)
//                await MarkLeave(empCode, before);

//            // SANDWICH AFTER
//            DateTime after = end.AddDays(1);
//            if (after.DayOfWeek == DayOfWeek.Sunday)
//                await MarkLeave(empCode, after);

//            await _context.SaveChangesAsync();
//        }

//        private async Task MarkLeave(string empCode, DateTime date)
//        {
//            var row = await _context.Attendances
//                .FirstOrDefaultAsync(a => a.Emp_Code == empCode && a.Date == date);

//            if (row == null)
//            {
//                _context.Attendances.Add(new Attendance
//                {
//                    Emp_Code = empCode,
//                    Date = date,
//                    Status = "L",
//                    Total_Hours = 0
//                });
//            }
//            else
//            {
//                row.Status = "L";
//                row.Total_Hours = 0;
//            }
//        }

//        private async Task DeleteLeaveFromAttendance(Leave leave)
//        {
//            var emp = await _context.Employees.FindAsync(leave.EmployeeId);

//            var rec = _context.Attendances.Where(a =>
//                a.Emp_Code == emp.EmployeeCode &&
//                a.Date >= leave.StartDate &&
//                a.Date <= leave.EndDate);

//            _context.Attendances.RemoveRange(rec);
//            await _context.SaveChangesAsync();
//        }

//        // ---------------------------------------
//        // APPROVAL ENDPOINTS
//        // ---------------------------------------
//        public async Task<IActionResult> ManagerApprove(int id, bool approve, string remark)
//        {
//            await ProcessApproval(id, "Manager", approve, remark);
//            return RedirectToAction("ManagerApprovalList");
//        }

//        public async Task<IActionResult> HrApprove(int id, bool approve, string remark)
//        {
//            await ProcessApproval(id, "HR", approve, remark);
//            return RedirectToAction("HrApprovalList");
//        }

//        public async Task<IActionResult> VpApprove(int id, bool approve, string remark)
//        {
//            await ProcessApproval(id, "VP", approve, remark);
//            return RedirectToAction("VpApprovalList");
//        }

//        public async Task<IActionResult> DirectorApprove(int id, bool approve, string remark)
//        {
//            await ProcessApproval(id, "Director", approve, remark);
//            return RedirectToAction("DirectorApprovalList");
//        }
//    }
//}
//--------------------------------------------------old-
//using HRMS.Data;
//using HRMS.Models;
//using HRMS.Services;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace HRMS.Controllers
//{
//    public class LeaveController : Controller
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly INotificationService _notification;
//        private readonly IWorkflowService _workflow;

//        public LeaveController(ApplicationDbContext context, INotificationService notification, IWorkflowService workflow)
//        {
//            _context = context;
//            _notification = notification;
//            _workflow = workflow;
//        }

//        // -------------------- helpers --------------------
//        private int? GetCurrentEmployeeId()
//        {
//            return HttpContext.Session.GetInt32("EmployeeId");
//        }

//        private void UpdateOverallStatus(Leave leave)
//        {
//            if (leave.ManagerStatus == "Rejected" ||
//                leave.HrStatus == "Rejected" ||
//                leave.DirectorStatus == "Rejected")
//            {
//                leave.OverallStatus = "Rejected";
//            }
//            else if (leave.ManagerStatus == "Approved" &&
//                     leave.HrStatus == "Approved" &&
//                     leave.DirectorStatus == "Approved")
//            {
//                leave.OverallStatus = "Approved";
//            }
//            else
//            {
//                leave.OverallStatus = "Pending";
//            }
//        }

//        // -------------------- create leave --------------------

//        [HttpGet]
//        public IActionResult Create()
//        {
//            var empId = GetCurrentEmployeeId();
//            if (empId == null)
//                return RedirectToAction("Login", "Account");

//            var model = new Leave
//            {
//                StartDate = DateTime.Today,
//                EndDate = DateTime.Today
//            };
//            return View(model);
//        }
//        [HttpPost]
//        public async Task<IActionResult> Create(Leave model)
//        {
//            var empId = GetCurrentEmployeeId();
//            if (empId == null)
//               return RedirectToAction("Login", "Account");

//            var employee = await _context.Employees.FindAsync(empId);

//            model.EmployeeId = empId.Value;

//            if (!ModelState.IsValid)
//                return View(model);

//            model.StartDate = model.StartDate.Date;
//            if (model.EndDate.HasValue)
//                model.EndDate = model.EndDate.Value.Date;

//            if (!model.EndDate.HasValue || model.EndDate < model.StartDate)
//                model.EndDate = model.StartDate;

//            model.TotalDays = (model.EndDate.Value - model.StartDate).TotalDays + 1;

//            model.CreatedOn = DateTime.Now;

//            // default all pending
//            model.ManagerStatus = "Pending";
//            model.HrStatus = "Pending";
//            model.VpStatus = "Pending";
//            model.DirectorStatus = "Pending";


//            model.OverallStatus = "Pending";

//            model.ReportingManagerId = employee.ManagerId;

//            // -----------------------------------------
//            // ROLE-BASED WORKFLOW LOGIC (FINAL VERSION)
//            // -----------------------------------------
//            switch (employee.Role)
//            {
//                // ---------------- EMPLOYEE ----------------
//                case "Employee":
//                    model.CurrentApproverRole = "Employee";
//                    model.NextApproverRole = "HR";
//                    break;

//                // ---------------- HR ----------------
//                case "HR":
//                    // HR skips Manager (self)
//                    model.ManagerStatus = "Pending";     // Skip Manager
//                    model.HrStatus = "Pending";           // HR role itself is first approver?
//                    model.VpStatus = "Pending";
//                    model.DirectorStatus = "Pending";

//                    model.CurrentApproverRole = "HR";
//                    model.NextApproverRole = "GM";        // GM approves HR leave
//                    break;

//                // ---------------- GM ----------------
//                case "GM":
//                    model.ManagerStatus = "Approved";
//                    model.HrStatus = "Approved";
//                    model.VpStatus = "Approved";

//                    model.CurrentApproverRole = "GM";
//                    model.NextApproverRole = "Director";
//                    break;

//                // ---------------- VP ----------------
//                case "VP":
//                    model.ManagerStatus = "Approved";
//                    model.HrStatus = "Approved";

//                    model.CurrentApproverRole = "VP";
//                    model.NextApproverRole = "Director";
//                    break;

//                // ---------------- DIRECTOR ----------------
//                case "Director":
//                    model.ManagerStatus = "Approved";
//                    model.HrStatus = "Approved";
//                    model.VpStatus = "Approved";
//                    model.DirectorStatus = "Approved";

//                    model.OverallStatus = "Approved";
//                    model.CurrentApproverRole = "Director";
//                    model.NextApproverRole = "Completed";
//                    break;

//                default:
//                    model.CurrentApproverRole = "Employee";
//                    model.NextApproverRole = "HR";
//                    break;
//            }

//            _context.Leaves.Add(model);
//            await _context.SaveChangesAsync();

//            return RedirectToAction("MyLeaves");
//        }



//        public async Task<IActionResult> MyLeaves()
//        {
//            var empId = GetCurrentEmployeeId();
//            if (empId == null)
//                return RedirectToAction("Login", "Account");

//            var leaves = await _context.Leaves
//                .Where(l => l.EmployeeId == empId.Value)
//                .OrderByDescending(l => l.CreatedOn)
//                .ToListAsync();

//            return View(leaves);
//        }



//        [HttpGet]
//        public async Task<IActionResult> GetEmployeeList()
//        {
//            var list = await _context.Employees
//                .Select(e => new { id = e.Id, name = e.Name })
//                .OrderBy(e => e.name)
//                .ToListAsync();

//            return Json(list);
//        }




//        private async Task ProcessApproval(int id, string NextApproverRole, bool approve, string? remark)
//        {
//            var leave = await _context.Leaves.FirstOrDefaultAsync(l => l.Id == id);
//            if (leave == null) return;

//            // UPDATE STATUS
//            switch (NextApproverRole)
//            {
//                case "GM":
//                    leave.ManagerStatus = approve ? "Approved" : "Rejected";
//                    leave.ManagerRemark = remark;
//                    break;

//                case "HR":
//                    leave.HrStatus = approve ? "Approved" : "Rejected";
//                    leave.HrRemark = remark;
//                    break;

//                case "VP":
//                    leave.VpStatus = approve ? "Approved" : "Rejected";
//                    leave.VpRemark = remark;
//                    break;

//                case "Director":
//                    leave.DirectorStatus = approve ? "Approved" : "Rejected";
//                    leave.DirectorRemark = remark;
//                    break;
//            }

//            // IF REJECTED → DELETE ATTENDANCE
//            if (!approve)
//            {
//                leave.OverallStatus = "Rejected";
//                leave.NextApproverRole = "Completed";

//                await DeleteLeaveFromAttendance(leave);
//                await _context.SaveChangesAsync();
//                return;
//            }

//            // IF APPROVED → CONTINUE WORKFLOW
//            leave.CurrentApproverRole = NextApproverRole;
//            leave.NextApproverRole = await _workflow.GetNextApproverRoleAsync(NextApproverRole);

//            // FINAL APPROVAL
//            if (leave.NextApproverRole == "Completed")
//            {
//                leave.OverallStatus = "Approved";

//                // 🟢 ATTENDANCE UPDATE HERE
//                await SyncLeaveToAttendance(leave);
//            }

//            await _context.SaveChangesAsync();
//        }
//        private async Task SyncLeaveToAttendance(Leave leave)
//        {
//            var employee = await _context.Employees.FindAsync(leave.EmployeeId);
//            if (employee == null) return;

//            string empCode = employee.EmployeeCode;

//            // MAIN LEAVE RANGE
//            DateTime start = leave.StartDate.Date;
//            DateTime end = leave.EndDate?.Date ?? leave.StartDate.Date;

//            // FIRST APPLY DIRECT LEAVE DAYS
//            for (var date = start; date <= end; date = date.AddDays(1))
//            {
//                await MarkLeave(empCode, date);
//            }

//            // 🟦 SANDWICH RULE: Check days BEFORE the leave
//            DateTime dayBefore = start.AddDays(-1);
//            if (await IsWeeklyOff(dayBefore))
//            {
//                // Find the closest working day before
//                var previousWorkingDay = await FindPreviousWorkingDay(empCode, dayBefore);

//                if (previousWorkingDay != null)
//                {
//                    var attendancePrev = await _context.Attendances
//                        .FirstOrDefaultAsync(a => a.Emp_Code == empCode && a.Date == previousWorkingDay);

//                    // If previous working day is Leave → sandwich before applies
//                    if (attendancePrev != null && attendancePrev.Status == "L")
//                    {
//                        await MarkLeave(empCode, dayBefore);
//                    }
//                }
//            }

//            // 🟦 SANDWICH RULE: Check days AFTER the leave
//            DateTime dayAfter = end.AddDays(1);
//            if (await IsWeeklyOff(dayAfter))
//            {
//                // Find next working day after
//                var nextWorkingDay = await FindNextWorkingDay(empCode, dayAfter);

//                var attendanceNext = await _context.Attendances
//                    .FirstOrDefaultAsync(a => a.Emp_Code == empCode && a.Date == nextWorkingDay);

//                if (attendanceNext != null && attendanceNext.Status == "L")
//                {
//                    await MarkLeave(empCode, dayAfter);
//                }
//            }

//            await _context.SaveChangesAsync();
//        }
//        private async Task MarkLeave(string empCode, DateTime date)
//        {
//            var record = await _context.Attendances
//                .FirstOrDefaultAsync(a => a.Emp_Code == empCode && a.Date == date);

//            if (record == null)
//            {
//                record = new Attendance
//                {
//                    Emp_Code = empCode,
//                    Date = date,
//                    Status = "L",
//                    InTime = null,
//                    OutTime = null,
//                    Total_Hours = 0
//                };
//                _context.Attendances.Add(record);
//            }
//            else
//            {
//                record.Status = "L";
//                record.InTime = null;
//                record.OutTime = null;
//                record.Total_Hours = 0;
//            }
//        }
//        private async Task<DateTime> FindNextWorkingDay(string empCode, DateTime from)
//        {
//            for (var d = from.AddDays(1); d <= from.AddDays(7); d = d.AddDays(1))
//            {
//                if (!await IsWeeklyOff(d))
//                    return d;
//            }
//            return from;
//        }

//        private async Task<DateTime?> FindPreviousWorkingDay(string empCode, DateTime from)
//        {
//            for (var d = from.AddDays(-1); d >= from.AddDays(-7); d = d.AddDays(-1))
//            {
//                if (!await IsWeeklyOff(d))
//                    return d;
//            }
//            return null;
//        }

//        private Task<bool> IsWeeklyOff(DateTime date)
//        {
//            return Task.FromResult(
//                date.DayOfWeek == DayOfWeek.Sunday
//            // OR include Saturday:
//            // || date.DayOfWeek == DayOfWeek.Saturday
//            );
//        }

//        private async Task DeleteLeaveFromAttendance(Leave leave)
//        {
//            var employee = await _context.Employees.FindAsync(leave.EmployeeId);
//            if (employee == null) return;

//            string empCode = employee.EmployeeCode;

//            var records = _context.Attendances
//                .Where(a => a.Emp_Code == empCode &&
//                            a.Date >= leave.StartDate &&
//                            a.Date <= leave.EndDate);

//            _context.Attendances.RemoveRange(records);
//            await _context.SaveChangesAsync();
//        }

//        //private async Task ProcessApproval(int id, string NextApproverRole, bool approve, string? remark)
//        //{
//        //    var leave = await _context.Leaves.FirstOrDefaultAsync(l => l.Id == id);
//        //    if (leave == null) return;

//        //    switch (NextApproverRole)
//        //    {
//        //        case "Manager":
//        //            leave.ManagerStatus = approve ? "Approved" : "Rejected";
//        //            leave.ManagerRemark = remark;
//        //            break;

//        //        case "HR":
//        //            leave.HrStatus = approve ? "Approved" : "Rejected";
//        //            leave.HrRemark = remark;
//        //            break;

//        //        case "VP":
//        //            leave.VpStatus = approve ? "Approved" : "Rejected";
//        //            leave.VpRemark = remark;
//        //            break;

//        //        case "Director":
//        //            leave.DirectorStatus = approve ? "Approved" : "Rejected";
//        //            leave.DirectorRemark = remark;
//        //            break;
//        //    }

//        //    // If REJECTED
//        //    if (!approve)
//        //    {
//        //        leave.OverallStatus = "Rejected";
//        //        leave.NextApproverRole = "Completed";

//        //        // 🔴 Remove attendance rows
//        //        await DeleteLeaveFromAttendance(leave);

//        //        await _context.SaveChangesAsync();
//        //        return;
//        //    }

//        //    // If APPROVED
//        //    leave.CurrentApproverRole = NextApproverRole;
//        //    leave.NextApproverRole = await _workflow.GetNextApproverRoleAsync(NextApproverRole);

//        //    // Fully approved
//        //    if (leave.NextApproverRole == "Completed")
//        //    {
//        //        leave.OverallStatus = "Approved";

//        //        // 🟢 Sync leave to attendance
//        //        await SyncLeaveToAttendance(leave);
//        //    }

//        //    await _context.SaveChangesAsync();
//        //}
//        // =======================================================
//        // 🟢 INSERT / UPDATE ATTENDANCE WHEN LEAVE IS APPROVED
//        // =======================================================
//        //private async Task SyncLeaveToAttendance(Leave leave)
//        //{
//        //    var employee = await _context.Employees.FindAsync(leave.EmployeeId);
//        //    if (employee == null) return;

//        //    string empCode = employee.EmployeeCode;

//        //    for (var date = leave.StartDate.Date; date <= leave.EndDate.Value.Date; date = date.AddDays(1))
//        //    {
//        //        var record = await _context.Attendances
//        //            .FirstOrDefaultAsync(a => a.Emp_Code == empCode && a.Date == date);

//        //        if (record == null)
//        //        {
//        //            // Create leave entry
//        //            record = new Attendance
//        //            {
//        //                Emp_Code = empCode,
//        //                Date = date,
//        //                Status = "L",
//        //                InTime = null,
//        //                OutTime = null,
//        //                Total_Hours = 0
//        //            };
//        //            _context.Attendances.Add(record);
//        //        }
//        //        else
//        //        {
//        //            // Update existing row to Leave
//        //            record.Status = "L";
//        //            record.InTime = null;
//        //            record.OutTime = null;
//        //            record.Total_Hours = 0;
//        //        }
//        //    }

//        //    await _context.SaveChangesAsync();
//        //}


//        // =======================================================
//        // 🔴 DELETE ATTENDANCE WHEN LEAVE IS REJECTED
//        // =======================================================
//        //private async Task DeleteLeaveFromAttendance(Leave leave)
//        //{
//        //    var employee = await _context.Employees.FindAsync(leave.EmployeeId);
//        //    if (employee == null) return;

//        //    string empCode = employee.EmployeeCode;

//        //    var records = _context.Attendances
//        //        .Where(a => a.Emp_Code == empCode &&
//        //                    a.Date >= leave.StartDate &&
//        //                    a.Date <= leave.EndDate);

//        //    _context.Attendances.RemoveRange(records);

//        //    await _context.SaveChangesAsync();
//        //}

//        //private async Task ProcessApproval(int id, string NextApproverRole, bool approve, string? remark)
//        //{
//        //    var leave = await _context.Leaves.FirstOrDefaultAsync(l => l.Id == id);
//        //    if (leave == null) return;

//        //    switch (NextApproverRole)
//        //    {
//        //        case "Manager":
//        //            leave.ManagerStatus = approve ? "Approved" : "Rejected";
//        //            leave.ManagerRemark = remark;
//        //            break;

//        //        case "HR":
//        //            leave.HrStatus = approve ? "Approved" : "Rejected";
//        //            leave.HrRemark = remark;
//        //            break;

//        //        case "VP":
//        //            leave.VpStatus  = approve ? "Approved" : "Rejected";
//        //            leave.VpRemark  = remark;
//        //            break;

//        //        case "Director":
//        //            leave.DirectorStatus = approve ? "Approved" : "Rejected";
//        //            leave.DirectorRemark = remark;
//        //            break;
//        //    }

//        //    if (!approve)
//        //    {
//        //        leave.OverallStatus = "Rejected";
//        //        leave.NextApproverRole = "Completed";
//        //    }
//        //    else
//        //    {
//        //        leave.CurrentApproverRole = NextApproverRole;
//        //        leave.NextApproverRole = await _workflow.GetNextApproverRoleAsync(NextApproverRole);

//        //        if (leave.NextApproverRole == "Completed")
//        //            leave.OverallStatus = "Approved";
//        //    }

//        //    await _context.SaveChangesAsync();
//        //}

//        // -------------------- MANAGER APPROVAL --------------------
//        public async Task<IActionResult> ManagerApprove(int id, bool approve, string? remark)
//        {
//            await ProcessApproval(id, "Manager", approve, remark);
//            return RedirectToAction("ManagerApprovalList");
//        }

//        // -------------------- HR APPROVAL --------------------
//        public async Task<IActionResult> HrApprove(int id, bool approve, string? remark)
//        {
//            await ProcessApproval(id, "HR", approve, remark);
//            return RedirectToAction("HrApprovalList");
//        }

//        public async Task<IActionResult> VpApprove(int id, bool approve, string? remark)
//        {
//            await ProcessApproval(id, "VP", approve, remark);
//            return RedirectToAction("VpApprovalList");
//        }
//        // -------------------- DIRECTOR APPROVAL --------------------
//        public async Task<IActionResult> DirectorApprove(int id, bool approve, string? remark)
//        {
//            await ProcessApproval(id, "Director", approve, remark);
//            return RedirectToAction("DirectorApprovalList");
//        }
//        public async Task<IActionResult> ApprovalDashboard()
//        {
//            var empId = GetCurrentEmployeeId();
//            if (empId == null)
//                return RedirectToAction("Login", "Account");

//            var employee = await _context.Employees.FindAsync(empId);

//            if (employee == null)
//                return RedirectToAction("Login", "Account");

//            // Decide which approval list to show based on role
//            return employee.Role switch
//            {

//                "HR" => RedirectToAction("HrApprovalList"),
//                "GM" => RedirectToAction("ManagerApprovalList"),
//                "VP" => RedirectToAction("VpApprovalList"),
//                "Director" => RedirectToAction("DirectorApprovalList"),
//                _ => RedirectToAction("LeaveReport") // Employee → No approval dashboard
//            };
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetPendingCount()
//        {
//            var empId = GetCurrentEmployeeId();
//            if (empId == null) return Json(0);

//            var emp = await _context.Employees.FindAsync(empId);

//            int count = emp.Role switch
//            {
//                "HR" => await _context.Leaves.CountAsync(l => l.HrStatus == "Pending"  && l.CurrentApproverRole == emp.Role),
//                "Manager" => await _context.Leaves.CountAsync(l => l.ManagerStatus == "Pending" && l.HrStatus == "Approved"),
//                "VP" => await _context.Leaves.CountAsync(l => l.ManagerStatus == "Approved" && l.HrStatus == "Approved" && l.VpStatus == "Pending"),
//                "Director" => await _context.Leaves.CountAsync(l =>
//                                l.ManagerStatus == "Approved" &&
//                                l.HrStatus == "Approved" &&
//                                l.VpStatus == "Approved" &&
//                                l.DirectorStatus == "Pending"),
//                _ => 0
//            };

//            return Json(count);
//        }
//        public async Task<IActionResult> Dashboard()
//        {
//            var empId = HttpContext.Session.GetInt32("EmployeeId");
//            var emp = await _context.Employees.FindAsync(empId);

//            // Pass user role to view
//            ViewBag.UserRole = emp.Role;

//            // Pass pending counts
//            ViewBag.ManagerPending = await _context.Leaves.CountAsync(l => l.ManagerStatus == "Pending");
//            ViewBag.HrPending = await _context.Leaves.CountAsync(l => l.ManagerStatus == "Approved" && l.HrStatus == "Pending");
//            ViewBag.VpPending = await _context.Leaves.CountAsync(l => l.ManagerStatus == "Approved" && l.HrStatus == "Approved" && l.VpStatus == "Pending");
//            ViewBag.DirectorPending = await _context.Leaves.CountAsync(l =>
//                                l.ManagerStatus == "Approved" &&
//                                l.HrStatus == "Approved" &&
//                                l.VpStatus == "Approved" &&
//                                l.DirectorStatus == "Pending");

//            return View();
//        }


//    }
//}


