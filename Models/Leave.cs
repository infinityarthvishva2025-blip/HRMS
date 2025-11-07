using System;
using System.ComponentModel.DataAnnotations;

namespace HRMS.Models
{
    public class Leave
    {
        [Key]
        public int Id { get; set; }   // EF will auto-detect this as an identity column

        [Required]
        [StringLength(50)]
        public string LeaveType { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [StringLength(200)]
        public string Reason { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Pending";
    }
}
