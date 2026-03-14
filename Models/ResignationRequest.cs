//using HRMS.Models;

//public class ResignationRequest
//{
//    public int Id { get; set; }

//    public int EmployeeId { get; set; }
//    public Employee Employee { get; set; }

//    public DateTime? ResignationDate { get; set; }

//    public DateTime SuggestedLastWorkingDay { get; set; }

//    public DateTime? ApprovedLastWorkingDay { get; set; }

//    public string? ReasonCode { get; set; }

//    public string? DetailedReason { get; set; }

//    public string? LetterPath { get; set; }

//    public int CurrentStep { get; set; } = 1;

//    public ResignationStatus Status { get; set; } = ResignationStatus.InApproval;

//   // public DateTime CreatedOn { get; set; } = DateTime.Now;

//    public int NoticePeriodDays { get; set; }

//    public DateTime? LastWorkingDate { get; set; }

//    public bool RelievingLetterSent { get; set; } = false;


//  //  public DateTime? LastWorkingDay { get; set; }
//    public DateTime? CreatedOn { get; set; } = DateTime.Now;
//}
using HRMS.Models;

public class ResignationRequest
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }
    public Employee Employee { get; set; }

    public DateTime? ResignationDate { get; set; }
    //public DateTime? ProposedLastWorkingDay { get; set; }
    public DateTime SuggestedLastWorkingDay { get; set; }   // ✅ ADD THIS
    public DateTime? ApprovedLastWorkingDay { get; set; }

    // ✅ ADD THESE BACK
    public string? ReasonCode { get; set; }
    public string? DetailedReason { get; set; }

    public int? CurrentStep { get; set; } = 1;
    public ResignationStatus? Status { get; set; } = ResignationStatus.InApproval;

    public DateTime? CreatedOn { get; set; } = DateTime.Now;
    public bool RelievingLetterGenerated { get; set; } = false;

    public int? NoticePeriodDays { get; set; }

    public DateTime? LastWorkingDate { get; set; }
    public bool? RelievingLetterSent { get; set; } = false;
}
