using System;
using System.ComponentModel.DataAnnotations;

namespace HRMS.Models
{
    public enum HalfDayType
    {
        None,
        Morning,
        Afternoon
    }

    public class Leave
    {
        [Key]
        public int Id { get; set; }

        [Required, Display(Name = "Leave Type")]
        public string LeaveType { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Display(Name = "From Date")]
        public DateTime? FromDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "To Date")]
        public DateTime? ToDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Leave Date (Half Day)")]
        public DateTime? LeaveDate { get; set; }

        [Display(Name = "Half Day Slot")]
        public HalfDayType HalfDayType { get; set; } = HalfDayType.None;

        [Required, StringLength(250)]
        public string Reason { get; set; } = string.Empty;

        public string EmployeeCode { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = "Pending";
    }
}
