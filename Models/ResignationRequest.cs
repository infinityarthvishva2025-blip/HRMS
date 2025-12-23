using HRMS.Models;

public class ResignationRequest
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }
    public Employee Employee { get; set; }

    public DateTime? ResignationDate { get; set; }
    public DateTime? ProposedLastWorkingDay { get; set; }
    public DateTime? ApprovedLastWorkingDay { get; set; }

    // ✅ ADD THESE BACK
    public string? ReasonCode { get; set; }
    public string? DetailedReason { get; set; }

    public int CurrentStep { get; set; } = 1;
    public ResignationStatus Status { get; set; } = ResignationStatus.InApproval;

    public DateTime CreatedOn { get; set; } = DateTime.Now;
}
