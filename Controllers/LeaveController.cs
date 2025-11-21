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
        private readonly INotificationService _notification;
        private readonly IWorkflowService _workflow;

        public LeaveController(ApplicationDbContext context, INotificationService notification, IWorkflowService workflow)
        {
            _context = context;
            _notification = notification;
            _workflow = workflow;
        }
        //public LeaveController(ApplicationDbContext context, INotificationService notification)
        //{
        //    _context = context;
        //    _notification = notification;
        //}

        // -------------------- helpers --------------------
        private int? GetCurrentEmployeeId()
        {
            return HttpContext.Session.GetInt32("EmployeeId");
        }

        private void UpdateOverallStatus(Leave leave)
        {
            if (leave.ManagerStatus == "Rejected" ||
                leave.HrStatus == "Rejected" ||
                leave.DirectorStatus == "Rejected")
            {
                leave.OverallStatus = "Rejected";
            }
            else if (leave.ManagerStatus == "Approved" &&
                     leave.HrStatus == "Approved" &&
                     leave.DirectorStatus == "Approved")
            {
                leave.OverallStatus = "Approved";
            }
            else
            {
                leave.OverallStatus = "Pending";
            }
        }

        // -------------------- create leave --------------------

        [HttpGet]
        public IActionResult Create()
        {
            var empId = GetCurrentEmployeeId();
            if (empId == null)
                return RedirectToAction("Login", "Account");

            var model = new Leave
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Create(Leave model)
        {
            var empId = GetCurrentEmployeeId();
            if (empId == null)
                return RedirectToAction("Login", "Account");

            var employee = await _context.Employees.FindAsync(empId);

            model.EmployeeId = empId.Value;

            if (!ModelState.IsValid)
                return View(model);

            model.StartDate = model.StartDate.Date;
            if (model.EndDate.HasValue)
                model.EndDate = model.EndDate.Value.Date;

            if (!model.EndDate.HasValue || model.EndDate < model.StartDate)
                model.EndDate = model.StartDate;

            model.TotalDays = (model.EndDate.Value - model.StartDate).TotalDays + 1;

            model.CreatedOn = DateTime.Now;

            // default all pending
            model.ManagerStatus = "Pending";
            model.HrStatus = "Pending";
            model.VpStatus = "Pending";
            model.DirectorStatus = "Pending";
            model.OverallStatus = "Pending";

            model.ReportingManagerId = employee.ManagerId;

            // -----------------------------------------
            // ROLE-BASED WORKFLOW LOGIC (FINAL VERSION)
            // -----------------------------------------
            switch (employee.Role)
            {
                // ---------------- EMPLOYEE ----------------
                case "Employee":
                    model.CurrentApproverRole = "Employee";
                    model.NextApproverRole = "Manager";
                    break;

                // ---------------- HR ----------------
                case "HR":
                    // HR skips Manager (self)
                    model.ManagerStatus = "Approved";     // Skip Manager
                    model.HrStatus = "Pending";           // HR role itself is first approver?
                    model.VpStatus = "Pending";
                    model.DirectorStatus = "Pending";

                    model.CurrentApproverRole = "HR";
                    model.NextApproverRole = "GM";        // GM approves HR leave
                    break;

                // ---------------- GM ----------------
                case "GM":
                    model.ManagerStatus = "Approved";
                    model.HrStatus = "Approved";
                    model.VpStatus = "Approved";

                    model.CurrentApproverRole = "GM";
                    model.NextApproverRole = "Director";
                    break;

                // ---------------- VP ----------------
                case "VP":
                    model.ManagerStatus = "Approved";
                    model.HrStatus = "Approved";

                    model.CurrentApproverRole = "VP";
                    model.NextApproverRole = "Director";
                    break;

                // ---------------- DIRECTOR ----------------
                case "Director":
                    model.ManagerStatus = "Approved";
                    model.HrStatus = "Approved";
                    model.VpStatus = "Approved";
                    model.DirectorStatus = "Approved";

                    model.OverallStatus = "Approved";
                    model.CurrentApproverRole = "Director";
                    model.NextApproverRole = "Completed";
                    break;

                default:
                    model.CurrentApproverRole = "Employee";
                    model.NextApproverRole = "Manager";
                    break;
            }

            _context.Leaves.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("MyLeaves");
        }

        

        public async Task<IActionResult> MyLeaves()
        {
            var empId = GetCurrentEmployeeId();
            if (empId == null)
                return RedirectToAction("Login", "Account");

            var leaves = await _context.Leaves
                .Where(l => l.EmployeeId == empId.Value)
                .OrderByDescending(l => l.CreatedOn)
                .ToListAsync();

            return View(leaves);
        }

        // -------------------- MANAGER APPROVAL --------------------
      //  [AuthorizeRole("Manager")]
        public async Task<IActionResult> ManagerApprovalList()
        {
            // You can filter by manager's team here if you store ManagerId
            var pending = await _context.Leaves
                .Include(l => l.Employee)
                .Where(l => l.ManagerStatus == "Pending")
                .OrderByDescending(l => l.CreatedOn)
                .OrderBy(l => l.CreatedOn)
                .ToListAsync();

            ViewData["ApproverRole"] = "Manager";
            ViewData["ApproveAction"] = "ManagerApprove";

            return View("ApprovalList", pending);
        }
       
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> ManagerApprove(int id, bool approve, string? remark)
        //{
        //    var leave = await _context.Leaves
        //        .Include(l => l.Employee)
        //        .FirstOrDefaultAsync(l => l.Id == id);

        //    if (leave == null) return NotFound();

        //    leave.ManagerStatus = approve ? "Approved" : "Rejected";
        //    leave.ManagerRemark = remark;

        //    UpdateOverallStatus(leave);
        //    await _context.SaveChangesAsync();

        //    await _notification.NotifyLeaveStatusChangedAsync(leave, "Manager", approve);

        //    return RedirectToAction("ManagerApprovalList");
        //}

        // -------------------- HR APPROVAL --------------------
       // [AuthorizeRole("HR")]
        public async Task<IActionResult> HrApprovalList()
        {
            var pending = await _context.Leaves
                .Include(l => l.Employee)
                .Where(l => l.ManagerStatus == "Approved" &&
                            l.HrStatus == "Pending")
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
                            l.VpStatus == "Pending" &&
                            l.DirectorStatus == "Pending")
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
                             l.VpStatus  == "Approved" &&
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
                case "Manager":
                    // show all for manager (you can restrict to team if you have ManagerId)
                    break;

                case "HR":
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

            return PartialView("_ApprovalTablePartial", list);
        }
        public async Task<IActionResult> LeaveReport()
        {
            var now = DateTime.Today;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);

            var pendingManager = await _context.Leaves.CountAsync(l => l.ManagerStatus == "Pending");
            var pendingHr = await _context.Leaves.CountAsync(l => l.ManagerStatus == "Approved" &&
                                                                   l.HrStatus == "Pending");
            var pendingDirector = await _context.Leaves.CountAsync(l => l.ManagerStatus == "Approved" &&
                                                                         l.HrStatus == "Approved" &&
                                                                         l.DirectorStatus == "Pending");
            var totalThisMonth = await _context.Leaves.CountAsync(l => l.StartDate >= startOfMonth &&
                                                                        l.StartDate <= now);

            ViewBag.PendingManager = pendingManager;
            ViewBag.PendingHr = pendingHr;
            ViewBag.PendingDirector = pendingDirector;
            ViewBag.TotalThisMonth = totalThisMonth;
           // var leaves = await _context.Leaves.ToListAsync();
            //return View();


            var empId = GetCurrentEmployeeId();
            if (empId == null)
                return RedirectToAction("Login", "Account");

            var leaves = await _context.Leaves
                .Where(l => l.EmployeeId == empId.Value)
                .OrderByDescending(l => l.CreatedOn)
                .ToListAsync();

            return View(leaves);
        }

        [HttpGet]
        public async Task<IActionResult> GetMonthlyLeaveSummary(int? year)
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
        [HttpGet]
        public async Task<IActionResult> GetEmployeeList()
        {
            var list = await _context.Employees
                .Select(e => new { id = e.Id, name = e.Name })
                .OrderBy(e => e.name)
                .ToListAsync();

            return Json(list);
        }

       

        [HttpGet]
        public async Task<IActionResult> GetPendingCounts()
        {
            var manager = await _context.Leaves.CountAsync(l => l.ManagerStatus == "Pending");

            var hr = await _context.Leaves.CountAsync(l =>
                l.ManagerStatus == "Approved" &&
                l.HrStatus == "Pending");

            var director = await _context.Leaves.CountAsync(l =>
                l.ManagerStatus == "Approved" &&
                l.HrStatus == "Approved" &&
                l.DirectorStatus == "Pending");

            return Json(new
            {
                managerPending = manager,
                hrPending = hr,
                directorPending = director
            });
        }

        private async Task ProcessApproval(int id, string NextApproverRole, bool approve, string? remark)
        {
            var leave = await _context.Leaves.FirstOrDefaultAsync(l => l.Id == id);
            if (leave == null) return;

            switch (NextApproverRole)
            {
                case "Manager":
                    leave.ManagerStatus = approve ? "Approved" : "Rejected";
                    leave.ManagerRemark = remark;
                    break;

                case "HR":
                    leave.HrStatus = approve ? "Approved" : "Rejected";
                    leave.HrRemark = remark;
                    break;

                case "VP":
                    leave.VpStatus  = approve ? "Approved" : "Rejected";
                    leave.VpRemark  = remark;
                    break;

                case "Director":
                    leave.DirectorStatus = approve ? "Approved" : "Rejected";
                    leave.DirectorRemark = remark;
                    break;
            }

            if (!approve)
            {
                leave.OverallStatus = "Rejected";
                leave.NextApproverRole = "Completed";
            }
            else
            {
                leave.CurrentApproverRole = NextApproverRole;
                leave.NextApproverRole = await _workflow.GetNextApproverRoleAsync(NextApproverRole);

                if (leave.NextApproverRole == "Completed")
                    leave.OverallStatus = "Approved";
            }

            await _context.SaveChangesAsync();
        }

        // -------------------- MANAGER APPROVAL --------------------
        public async Task<IActionResult> ManagerApprove(int id, bool approve, string? remark)
        {
            await ProcessApproval(id, "Manager", approve, remark);
            return RedirectToAction("ManagerApprovalList");
        }

        // -------------------- HR APPROVAL --------------------
        public async Task<IActionResult> HrApprove(int id, bool approve, string? remark)
        {
            await ProcessApproval(id, "HR", approve, remark);
            return RedirectToAction("HrApprovalList");
        }
       
        public async Task<IActionResult> VpApprove(int id, bool approve, string? remark)
        {
            await ProcessApproval(id, "VP", approve, remark);
            return RedirectToAction("VpApprovalList");
        }
        // -------------------- DIRECTOR APPROVAL --------------------
        public async Task<IActionResult> DirectorApprove(int id, bool approve, string? remark)
        {
            await ProcessApproval(id, "Director", approve, remark);
            return RedirectToAction("DirectorApprovalList");
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
                "Manager" => await _context.Leaves.CountAsync(l => l.ManagerStatus == "Pending" && l.ReportingManagerId == empId),
                "HR" => await _context.Leaves.CountAsync(l => l.ManagerStatus == "Approved" && l.HrStatus == "Pending"),
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

            // Pass user role to view
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


    }
}


