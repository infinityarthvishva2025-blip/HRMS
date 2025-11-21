using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.Models
{
    public enum LeaveCategory
    {
        FullDay = 1,
        MultiDay = 2,
        HalfDay = 3,
        EarlyGoing = 4,
        LateComing = 5
    }

    public class Leave
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        // Category selected by Employee
        [Required]
        public LeaveCategory Category { get; set; }

        // Leave Type (Sick, Casual, Paid)
        public string? LeaveType { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        // Half Day Session
        public string? HalfDaySession { get; set; }  // FirstHalf / SecondHalf

        // EarlyGoing / LateComing time
        public TimeSpan? TimeValue { get; set; }

        //[Required]
        //public string Reason { get; set; }

        public string? ContactDuringLeave { get; set; }
        public string? AddressDuringLeave { get; set; }

        public double TotalDays { get; set; }

        // Approval statuses
        public string ManagerStatus { get; set; } = "Pending";
        public string HrStatus { get; set; } = "Pending";
        public string? VpStatus { get; set; } = "Pending";
        public string DirectorStatus { get; set; } = "Pending";

        public string OverallStatus { get; set; } = "Pending";

        public string CurrentApproverRole { get; set; } = "Employee";
        public string? NextApproverRole { get; set; } = "Manager";

        public DateTime CreatedOn { get; set; } = DateTime.Now;

        public string? ManagerRemark { get; set; }
        public string? HrRemark { get; set; }
        public string? VpRemark { get; set; }
        public string? DirectorRemark { get; set; }
     
        public string Reason { get; set; } = string.Empty;

       // public string Role { get; set; } = string.Empty;

        public int? ReportingManagerId { get; set; } // FIXED
    }

}

//using System;
//using System.ComponentModel.DataAnnotations;

//namespace HRMS.Models
//{
//    public enum LeaveCategory
//    {
//        FullDay,
//        MultiDay,
//        HalfDay,
//        EarlyGoing,
//        LateComing
//    }

//    public class Leave
//    {
//        public int Id { get; set; }

//        public int EmployeeId { get; set; }
//        public Employee? Employee { get; set; }
//        [Required]
//        public LeaveCategory Category { get; set; }

//        [Required]
//        public DateTime StartDate { get; set; }

//        public DateTime? EndDate { get; set; }

//        public string? HalfDaySession { get; set; } // FirstHalf / SecondHalf

//        public TimeSpan? TimeValue { get; set; } // Early / Late Time

//        [Required]
//        public string Reason { get; set; }

//        public string? ContactDuringLeave { get; set; }

//        public string? AddressDuringLeave { get; set; }

//        public double TotalDays { get; set; }

//        public string ManagerStatus { get; set; } = "Pending";

//        public string HrStatus { get; set; } = "Pending";

//        public string DirectorStatus { get; set; } = "Pending";

//        public string OverallStatus { get; set; } = "Pending";

//        public DateTime CreatedOn { get; set; } = DateTime.Now;
//        public string? ManagerRemark { get; internal set; }
//        public string? DirectorRemark { get; internal set; }
//        public string? HrRemark { get; internal set; }
//        public string? CurrentApproverRole { get; set; } = "Employee";
//        public string? NextApproverRole { get; set; } = "Manager";
//    }
//}
