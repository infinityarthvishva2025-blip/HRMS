using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.Models
{
    public class Leave
    {
        [Key]
        public int Id { get; set; }

        [Required, Display(Name = "Leave Type")]
        public string LeaveType { get; set; } = string.Empty;

        [Required, DataType(DataType.Date), Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Required, DataType(DataType.Date), Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [Required, StringLength(500), Display(Name = "Reason")]
        public string Reason { get; set; } = string.Empty;

        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending";

        // FK → Employee
        [Required, ForeignKey("Employee"), Display(Name = "Employee")]
        public int EmployeeId { get; set; }

        public Employee? Employee { get; set; }
      //  public Count? Count { get; set; }

        [NotMapped, Display(Name = "Total Days")]
        public int TotalDays => (EndDate - StartDate).Days + 1;

        public int Count { get; internal set; }
    }
}
