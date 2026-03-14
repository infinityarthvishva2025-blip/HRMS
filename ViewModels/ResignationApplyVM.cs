using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

public class ResignationApplyVM
{
    [Required]
    [DataType(DataType.Date)]
    public DateTime ResignationDate { get; set; }

    public int NoticePeriodDays { get; set; }
    public bool IsProvisionalCompleted { get; set; }

    [DataType(DataType.Date)]
    public DateTime SuggestedLastWorkingDay { get; set; }

    [Required(ErrorMessage = "Please select a reason for resignation")]
    public string ReasonCode { get; set; } = "";

    public string? DetailedReason { get; set; }

    public IFormFile? LetterFile { get; set; }
}
