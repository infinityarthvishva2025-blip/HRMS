using System.ComponentModel.DataAnnotations;

namespace HRMS.Models
{
    public class Leave
    {
        public int Id { get; set; }

        [Required]
        public string LeaveType { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public string Reason { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending";

        [Required]
        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }
        public int TotalDays { get; set; }

        public string? ContactDuringLeave { get; set; }

        public string? AddressDuringLeave { get; set; }


    }
}
