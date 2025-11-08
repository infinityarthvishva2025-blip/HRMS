using System;
using System.ComponentModel.DataAnnotations;

namespace HRMS.Models
{
    public class Leave
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Leave Type is required")]
        [Display(Name = "Leave Type")]
        public string LeaveType { get; set; }

        [Required(ErrorMessage = "Start Date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End Date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [Display(Name = "Reason")]
        public string Reason { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending";
    }
}
