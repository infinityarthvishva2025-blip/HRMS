using HRMS.Data;
using HRMS.Models;
using HRMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

[Authorize]
public class ResignationController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ResignationService _service;

    public ResignationController(ResignationService service, ApplicationDbContext context)
    {
        _service = service;
        _context = context;
    }

    // ==============================
    // APPLY (GET)
    // ==============================
    public IActionResult Apply()
    {
        var emp = GetLoggedInEmployee();

        if (!emp.JoiningDate.HasValue)
            throw new Exception("Joining Date not set. Contact HR.");

        var today = DateTime.Today;

        bool confirmed = (today - emp.JoiningDate.Value).TotalDays >= 90;
        int notice = confirmed ? 30 : 0;

        var vm = new ResignationApplyVM
        {
            ResignationDate = today,
            NoticePeriodDays = notice,
            IsProvisionalCompleted = confirmed,
            SuggestedLastWorkingDay = today.AddDays(notice),
            //ProposedLastWorkingDay = today.AddDays(notice)
        };

        return View(vm);
    }

    // ==============================
    // APPLY (POST)
    // ==============================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Apply(ResignationApplyVM vm)
    {
        var emp = GetLoggedInEmployee();

        bool confirmed = (vm.ResignationDate - emp.JoiningDate.Value).TotalDays >= 90;
        int notice = confirmed ? 30 : 0;

        vm.NoticePeriodDays = notice;
        vm.IsProvisionalCompleted = confirmed;

        // ✅ always calculate suggested
        vm.SuggestedLastWorkingDay = vm.ResignationDate.AddDays(notice);

        // ✅ IMPORTANT: clear old posted values (fix refresh-only issue)
        ModelState.Clear();
        TryValidateModel(vm);

        if (!ModelState.IsValid)
            return View(vm);

        var result = _service.Submit(vm, emp);
        if (!result.Success)
        {
            ModelState.AddModelError("", result.Error);
            return View(vm);
        }

        return RedirectToAction("Track");
    }



    // ==============================
    // EMPLOYEE TRACK
    // ==============================
    public IActionResult Track()
    {
        var emp = GetLoggedInEmployee();

        var list = _context.ResignationRequests
            .Where(r => r.EmployeeId == emp.Id)
            .OrderByDescending(r => r.CreatedOn)
            .ToList();

        var ids = list.Select(x => x.Id).ToList();

        ViewBag.Steps = _context.ResignationApprovalSteps
            .Where(s => ids.Contains(s.ResignationRequestId))
            .OrderBy(s => s.StepNo)
            .ToList();

        return View(list);
    }



    // ==============================
    // PENDING APPROVALS (FIXED)
    // ==============================
    public IActionResult PendingApprovals()
    {
        var emp = GetLoggedInEmployee();
        var role = emp.Role?.Trim();

        if (string.IsNullOrEmpty(role))
            return Unauthorized();

        // ============================
        // 1️⃣ BASE QUERY (SQL SAFE)
        // ============================
        var baseQuery = _context.ResignationApprovalSteps
     .Include(x => x.ResignationRequest)
         .ThenInclude(r => r.Employee)
     .Where(x =>
         x.Status == StepStatus.Pending &&
         x.ResignationRequest != null &&
         x.ResignationRequest.Employee != null &&
         x.ResignationRequest.CurrentStep != null &&
         x.StepNo == x.ResignationRequest.CurrentStep
     )
     .ToList(); // ✅ SAFE


        // ============================
        // 2️⃣ ROLE BASED FILTERING
        // ============================
        List<ResignationApprovalStep> approvals;

        if (role.Equals("Manager", StringComparison.OrdinalIgnoreCase))
        {
            // ✅ ONLY HIS EMPLOYEES
            approvals = baseQuery
                .Where(x =>
                    x.RoleName == "Manager" &&
                    x.ApproverEmployeeId == emp.Id
                )
                .ToList();
        }
        else
        {
            // ✅ HR / GM / VP / DIRECTOR
            approvals = baseQuery
                .Where(x =>
                    string.Equals(x.RoleName, role, StringComparison.OrdinalIgnoreCase)
                )
                .ToList();
        }

        return View(approvals);
    }





    // ==============================
    // APPROVE / REJECT (FIXED)
    // ==============================
    [HttpPost]
    public IActionResult ApproveReject(int stepId, bool approve, string? remarks)
    {
        var emp = GetLoggedInEmployee();

        var step = _context.ResignationApprovalSteps
            .Include(x => x.ResignationRequest)
            .FirstOrDefault(x => x.Id == stepId);

        if (step == null) return NotFound();

        // 🔐 SECURITY
        if (step.RoleName == "Manager" && step.ApproverEmployeeId != emp.Id)
            return Unauthorized();

        if (step.RoleName != "Manager" &&
            !string.Equals(step.RoleName, emp.Role, StringComparison.OrdinalIgnoreCase))
            return Unauthorized();

        if (step.StepNo != step.ResignationRequest.CurrentStep)
            return Unauthorized();

        if (approve)
        {
            step.Status = StepStatus.Approved;
            step.ActionOn = DateTime.Now;
            step.Remarks = remarks;

            bool hasNext = _context.ResignationApprovalSteps
                .Any(x => x.ResignationRequestId == step.ResignationRequestId &&
                          x.StepNo == step.StepNo + 1);

            if (hasNext)
                step.ResignationRequest.CurrentStep++;
            else
                step.ResignationRequest.Status = ResignationStatus.Approved;
        }
        else
        {
            step.Status = StepStatus.Rejected;
            step.ActionOn = DateTime.Now;
            step.Remarks = remarks;
            step.ResignationRequest.Status = ResignationStatus.Rejected;
        }

        _context.SaveChanges();
        return RedirectToAction("PendingApprovals");
    }


    // ==============================
    // LOGGED IN EMPLOYEE
    // ==============================
    private Employee GetLoggedInEmployee()
    {
        var empCode = User.Claims.FirstOrDefault(c => c.Type == "EmployeeCode")?.Value;

        if (string.IsNullOrEmpty(empCode))
            throw new Exception("EmployeeCode claim missing");

        var emp = _context.Employees.FirstOrDefault(e => e.EmployeeCode == empCode);

        if (emp == null)
            throw new Exception("Employee not found");

        return emp;
    }

}
