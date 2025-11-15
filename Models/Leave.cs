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

        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }
        [Required]
        public LeaveCategory Category { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string? HalfDaySession { get; set; } // FirstHalf / SecondHalf

        public TimeSpan? TimeValue { get; set; } // Early / Late Time

        [Required]
        public string Reason { get; set; }

        public string? ContactDuringLeave { get; set; }

        public string? AddressDuringLeave { get; set; }

        public double TotalDays { get; set; }

        public string ManagerStatus { get; set; } = "Pending";

        public string HrStatus { get; set; } = "Pending";

        public string DirectorStatus { get; set; } = "Pending";

        public string OverallStatus { get; set; } = "Pending";

        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public string? ManagerRemark { get; internal set; }
        public string? DirectorRemark { get; internal set; }
        public string? HrRemark { get; internal set; }
        public string? CurrentApproverRole { get; set; } = "Employee";
        public string? NextApproverRole { get; set; } = "Manager";
    }
}
