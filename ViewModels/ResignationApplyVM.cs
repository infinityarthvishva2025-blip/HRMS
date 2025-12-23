using System.ComponentModel.DataAnnotations;

public class ResignationApplyVM
{
    public DateTime ResignationDate { get; set; }

    public int NoticePeriodDays { get; set; }
    public bool IsProvisionalCompleted { get; set; }

    public DateTime SuggestedLastWorkingDay { get; set; }
    public DateTime ProposedLastWorkingDay { get; set; }

    [Required(ErrorMessage = "Please select a reason for resignation")]
    public string ReasonCode { get; set; }
    public string DetailedReason { get; set; }

    public IFormFile LetterFile { get; set; }
}
