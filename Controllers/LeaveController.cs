using System;
using System.Linq;
using System.Threading.Tasks;
using HRMS.Data;
using HRMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Controllers
{
    public class LeaveController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LeaveController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Helper – replace with your real logged-in employee logic
        private int GetLoggedInEmployeeId()
        {
            // TODO: lookup employee ID from User.Identity.Name
            return 1; // TEMP: hard-coded for testing
        }

        // ========== EMPLOYEE FORM ==========

        [HttpGet]
        public IActionResult Create()
        {
            var model = new Leave
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Leave model)
        {
            model.EmployeeId = GetLoggedInEmployeeId();

            if (!ModelState.IsValid)
                return View(model);

            // Normalize dates
            model.StartDate = model.StartDate.Date;
            if (model.EndDate.HasValue)
                model.EndDate = model.EndDate.Value.Date;

            // ===== Category-specific logic =====
            switch (model.Category)
            {
                case LeaveCategory.FullDay:
                    if (!model.EndDate.HasValue)
                        model.EndDate = model.StartDate;

                    if (model.EndDate.Value < model.StartDate)
                    {
                        ModelState.AddModelError(nameof(model.EndDate),
                            "End date must be on or after start date.");
                        return View(model);
                    }

                    model.TotalDays = (model.EndDate.Value - model.StartDate).TotalDays + 1;
                    break;

                case LeaveCategory.MultiDay:
                    if (!model.EndDate.HasValue)
                    {
                        ModelState.AddModelError(nameof(model.EndDate),
                            "Please select an end date for multi-day leave.");
                        return View(model);
                    }

                    if (model.EndDate.Value < model.StartDate)
                    {
                        ModelState.AddModelError(nameof(model.EndDate),
                            "End date must be on or after start date.");
                        return View(model);
                    }

                    model.TotalDays = (model.EndDate.Value - model.StartDate).TotalDays + 1;
                    break;

                case LeaveCategory.HalfDay:
                    model.EndDate = model.StartDate;
                    model.TotalDays = 0.5;
                    if (string.IsNullOrWhiteSpace(model.HalfDaySession))
                    {
                        ModelState.AddModelError(nameof(model.HalfDaySession),
                            "Please choose First Half or Second Half.");
                        return View(model);
                    }
                    break;

                case LeaveCategory.EarlyGoing:
                case LeaveCategory.LateComing:
                    model.EndDate = model.StartDate;
                    model.TotalDays = 0;
                    if (!model.TimeValue.HasValue)
                    {
                        ModelState.AddModelError(nameof(model.TimeValue),
                            "Please select a time.");
                        return View(model);
                    }
                    break;
            }

            // ===== Overlap check for Full / Multi / Half =====
            if (model.Category == LeaveCategory.FullDay
                || model.Category == LeaveCategory.MultiDay
                || model.Category == LeaveCategory.HalfDay)
            {
                var start = model.StartDate;
                var end = model.EndDate ?? model.StartDate;

                bool overlaps = await _context.Leaves.AnyAsync(l =>
                    l.EmployeeId == model.EmployeeId &&
                    (l.OverallStatus != "Rejected") &&
                    start <= (l.EndDate ?? l.StartDate) &&
                    end >= l.StartDate);

                if (overlaps)
                {
                    ModelState.AddModelError(string.Empty,
                        "These dates overlap with an existing leave request.");
                    return View(model);
                }
            }

            model.ManagerStatus = "Pending";
            model.HrStatus = "Pending";
            model.DirectorStatus = "Pending";
            model.OverallStatus = "Pending";

            _context.Leaves.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("MyLeaves");
        }

        // Shows logged-in employee's leaves
        public async Task<IActionResult> MyLeaves()
        {
            int empId = GetLoggedInEmployeeId();
            var data = await _context.Leaves
                .Include(l => l.Employee)
                .Where(l => l.EmployeeId == empId)
                .OrderByDescending(l => l.CreatedOn)
                .ToListAsync();

            return View(data);
        }

        // ========== APPROVAL FLOWS ==========

        // Generic helper to update OverallStatus
        private void UpdateOverallStatus(Leave leave)
        {
            if (leave.ManagerStatus == "Rejected"
                || leave.HrStatus == "Rejected"
                || leave.DirectorStatus == "Rejected")
            {
                leave.OverallStatus = "Rejected";
            }
            else if (leave.ManagerStatus == "Approved"
                     && leave.HrStatus == "Approved"
                     && leave.DirectorStatus == "Approved")
            {
                leave.OverallStatus = "Approved";
            }
            else
            {
                leave.OverallStatus = "Pending";
            }
        }

        // ========== Manager ==========

        public async Task<IActionResult> ManagerApprovalList()
        {
            // TODO: filter by team; for now: all pending for manager
            var pending = await _context.Leaves
                .Include(l => l.Employee)
                .Where(l => l.ManagerStatus == "Pending")
                .ToListAsync();

            return View("ApprovalList", pending);
        }

        [HttpPost]
        public async Task<IActionResult> ManagerApprove(int id, bool approve, string? remark)
        {
            var leave = await _context.Leaves.FindAsync(id);
            if (leave == null) return NotFound();

            leave.ManagerStatus = approve ? "Approved" : "Rejected";
            leave.ManagerRemark = remark;
            UpdateOverallStatus(leave);

            await _context.SaveChangesAsync();
            return RedirectToAction("ManagerApprovalList");
        }

        // ========== HR ==========

        public async Task<IActionResult> HrApprovalList()
        {
            var pending = await _context.Leaves
                .Include(l => l.Employee)
                .Where(l => l.ManagerStatus == "Approved" &&
                            l.HrStatus == "Pending")
                .ToListAsync();

            return View("ApprovalList", pending);
        }

        [HttpPost]
        public async Task<IActionResult> HrApprove(int id, bool approve, string? remark)
        {
            var leave = await _context.Leaves.FindAsync(id);
            if (leave == null) return NotFound();

            leave.HrStatus = approve ? "Approved" : "Rejected";
            leave.HrRemark = remark;
            UpdateOverallStatus(leave);

            await _context.SaveChangesAsync();
            return RedirectToAction("HrApprovalList");
        }

        // ========== Director ==========

        public async Task<IActionResult> DirectorApprovalList()
        {
            var pending = await _context.Leaves
                .Include(l => l.Employee)
                .Where(l => l.ManagerStatus == "Approved" &&
                            l.HrStatus == "Approved" &&
                            l.DirectorStatus == "Pending")
                .ToListAsync();

            return View("ApprovalList", pending);
        }

        [HttpPost]
        public async Task<IActionResult> DirectorApprove(int id, bool approve, string? remark)
        {
            var leave = await _context.Leaves.FindAsync(id);
            if (leave == null) return NotFound();

            leave.DirectorStatus = approve ? "Approved" : "Rejected";
            leave.DirectorRemark = remark;
            UpdateOverallStatus(leave);

            await _context.SaveChangesAsync();
            return RedirectToAction("DirectorApprovalList");
        }
    }
}
