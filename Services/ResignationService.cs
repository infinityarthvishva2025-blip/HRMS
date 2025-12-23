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
        // ❌ INTERN NOT ALLOWED
        if (emp.Role.Equals("Intern", StringComparison.OrdinalIgnoreCase))
            return (false, "Interns are not allowed to submit resignation.");

        // ❌ MANAGER MUST EXIST
        if (!emp.ManagerId.HasValue)
            return (false, "Reporting Manager is not assigned. Please contact HR.");

        // =========================
        // CREATE REQUEST
        // =========================
        var req = new ResignationRequest
        {
            EmployeeId = emp.Id,
            ResignationDate = vm.ResignationDate,
            ProposedLastWorkingDay = vm.ProposedLastWorkingDay,
            ReasonCode = vm.ReasonCode,
            DetailedReason = vm.DetailedReason,
            Status = ResignationStatus.InApproval,
            CurrentStep = 1,
            CreatedOn = DateTime.Now
        };

        _context.ResignationRequests.Add(req);
        _context.SaveChanges();

        int step = 1;

        // =========================
        // 1️⃣ MANAGER (ONLY HIS MANAGER)
        // =========================
        _context.ResignationApprovalSteps.Add(new ResignationApprovalStep
        {
            ResignationRequestId = req.Id,
            StepNo = step++,
            RoleName = "Manager",
            ApproverEmployeeId = emp.ManagerId.Value
        });

        // =========================
        // 2️⃣ HR → GM → VP → DIRECTOR
        // =========================
        var flow = new[] { "HR", "GM", "VP", "Director" };

        foreach (var role in flow)
        {
            _context.ResignationApprovalSteps.Add(new ResignationApprovalStep
            {
                ResignationRequestId = req.Id,
                StepNo = step++,
                RoleName = role
            });
        }

        _context.SaveChanges();
        return (true, string.Empty);
    }
}
