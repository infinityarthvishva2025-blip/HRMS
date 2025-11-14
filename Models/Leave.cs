using System;
using System.ComponentModel.DataAnnotations;

namespace HRMS.Models
{
    public enum LeaveCategory
    {
        FullDay,
        MultiDay,
        HalfDay,
        EarlyGoing,
        LateComing
    }

    public class Leave
    {
        public int Id { get; set; }

        // Who is applying
        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        [Required]
        public LeaveCategory Category { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }    // Used for FullDay + MultiDay

        // For half day
        public string? HalfDaySession { get; set; }   // "FirstHalf" / "SecondHalf"

        // For EarlyGoing / LateComing
        [DataType(DataType.Time)]
        public TimeSpan? TimeValue { get; set; }

        [Required]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;

        [StringLength(100)]
        public string? ContactDuringLeave { get; set; }

        [StringLength(500)]
        public string? AddressDuringLeave { get; set; }

        public double TotalDays { get; set; }

        // Approval workflow
        public string ManagerStatus { get; set; } = "Pending";   // Pending/Approved/Rejected
        public string HrStatus { get; set; } = "Pending";
        public string DirectorStatus { get; set; } = "Pending";
        public string OverallStatus { get; set; } = "Pending";

        public string? ManagerRemark { get; set; }
        public string? HrRemark { get; set; }
        public string? DirectorRemark { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
