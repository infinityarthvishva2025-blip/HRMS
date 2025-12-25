using HRMS.Data;
using HRMS.Models;

public class ResignationService
{
    private readonly ApplicationDbContext _context;

    private const int PROVISIONAL_DAYS = 90;
    private const int NOTICE_DAYS = 30;

    public ResignationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public (bool Success, string Error) Submit(ResignationApplyVM vm, Employee emp)
    {
        // ❌ Intern cannot resign
        if (!string.IsNullOrWhiteSpace(emp.Role) &&
            emp.Role.Equals("Intern", StringComparison.OrdinalIgnoreCase))
            return (false, "Interns are not allowed to submit resignation.");

        if (!emp.JoiningDate.HasValue)
            return (false, "Joining Date not set. Please contact HR.");

        if (vm.ResignationDate < DateTime.Today)
            return (false, "Resignation date cannot be in the past.");

        bool confirmed =
            (vm.ResignationDate - emp.JoiningDate.Value).TotalDays >= PROVISIONAL_DAYS;

        int noticeDays = confirmed ? NOTICE_DAYS : 0;

        DateTime suggestedLwd = vm.ResignationDate.AddDays(noticeDays);

        // ✅ FINAL SAFETY FIX
        //DateTime? proposedLwd = null;

        //if (vm.ProposedLastWorkingDay.HasValue &&
        //    vm.ProposedLastWorkingDay.Value != DateTime.MinValue)
        //{
        //    if (vm.ProposedLastWorkingDay.Value < suggestedLwd)
        //    {
        //        return (
        //            false,
        //            $"Proposed Last Working Day must be on or after {suggestedLwd:dd-MMM-yyyy}"
        //        );
        //    }

        //    proposedLwd = vm.ProposedLastWorkingDay.Value;
        //}

        // =========================
        // CREATE REQUEST
        // =========================
        var req = new ResignationRequest
        {
            EmployeeId = emp.Id,
            ResignationDate = vm.ResignationDate,

            // ✅ ALWAYS STORE
            SuggestedLastWorkingDay = suggestedLwd,

            // ✅ STORE ONLY IF EMPLOYEE SELECTED
            //ProposedLastWorkingDay = proposedLwd,

            ReasonCode = vm.ReasonCode,
            DetailedReason = vm.DetailedReason,
            Status = ResignationStatus.InApproval,
            CurrentStep = 1,
            CreatedOn = DateTime.Now
        };

        _context.ResignationRequests.Add(req);
        _context.SaveChanges();

        // =========================
        // BUILD APPROVAL FLOW
        // =========================
        var flow = BuildApprovalFlow(emp);

        if (flow.Count == 0)
            return (false, "No approval route found. Please contact HR.");

        int step = 1;
        foreach (var f in flow)
        {
            _context.ResignationApprovalSteps.Add(new ResignationApprovalStep
            {
                ResignationRequestId = req.Id,
                StepNo = step++,
                RoleName = f.RoleName,
                ApproverEmployeeId = f.ApproverEmployeeId
            });
        }

        _context.SaveChanges();
        return (true, string.Empty);
    }

    private List<(string RoleName, int? ApproverEmployeeId)> BuildApprovalFlow(Employee emp)
    {
        var role = (emp.Role ?? "Employee").Trim();
        var flow = new List<(string, int?)>();

        if (role.Equals("Employee", StringComparison.OrdinalIgnoreCase))
        {
            if (emp.ManagerId.HasValue)
                flow.Add(("Manager", emp.ManagerId.Value));

            flow.Add(("HR", null));
            flow.Add(("GM", null));
            flow.Add(("VP", null));
            flow.Add(("Director", null));
            return flow;
        }

        if (role.Equals("Manager", StringComparison.OrdinalIgnoreCase))
        {
            flow.Add(("HR", null));
            flow.Add(("GM", null));
            flow.Add(("VP", null));
            flow.Add(("Director", null));
            return flow;
        }

        if (role.Equals("HR", StringComparison.OrdinalIgnoreCase))
        {
            flow.Add(("GM", null));
            flow.Add(("VP", null));
            flow.Add(("Director", null));
            return flow;
        }

        if (role.Equals("GM", StringComparison.OrdinalIgnoreCase))
        {
            flow.Add(("VP", null));
            flow.Add(("Director", null));
            return flow;
        }

        if (role.Equals("VP", StringComparison.OrdinalIgnoreCase))
        {
            flow.Add(("Director", null));
            return flow;
        }

        return new();
    }
}
