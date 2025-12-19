using System.ComponentModel.DataAnnotations;

public class Holiday
{
    public int Id { get; set; }

    [Required]
    public DateTime HolidayDate { get; set; }

    [Required]
    public string HolidayName { get; set; }

    public string Description { get; set; }

    public string Status { get; set; }

    public DateTime CreatedOn { get; set; }

    public string ApprovedBy { get; set; }

    public DateTime? ApprovedOn { get; set; }
}
