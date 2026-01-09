using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.Models
{
    public class CompOffLedger
    {
        [Key]
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        [ForeignKey(nameof(EmployeeId))]
        public Employee Employee { get; set; }

        public DateTime EarnedDate { get; set; }

        public DateTime ExpiryDate { get; set; }

        // null = not used yet
        public DateTime? UsedDate { get; set; }

        // LeaveId that used it (optional but useful)
        public int? UsedLeaveId { get; set; }

        // Active / Expired / Used
        [MaxLength(20)]
        public string Status { get; set; } = "Active";

        [MaxLength(200)]
        public string? Remarks { get; set; }
    }
}
