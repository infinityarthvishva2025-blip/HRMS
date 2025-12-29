public class ResignationApprovalStep
{
    public int Id { get; set; }

    public int ResignationRequestId { get; set; }
    public ResignationRequest ResignationRequest { get; set; }

    public int StepNo { get; set; }

    // Manager / HR / GM / VP / Director
    public string RoleName { get; set; }

    // 🔥 NEW → used only for MANAGER step
    public int? ApproverEmployeeId { get; set; }

    public StepStatus Status { get; set; } = StepStatus.Pending;

    public DateTime? ActionOn { get; set; }
    public string? Remarks { get; set; }
}
