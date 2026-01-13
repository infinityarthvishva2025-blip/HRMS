using System.ComponentModel.DataAnnotations;

namespace HRMS.Models
{
    public class EmployeeReportVm
    {
        public string EmployeeCode { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
        public string MobileNumber { get; set; }
        public DateTime? JoiningDate { get; set; }
        public string Status { get; set; }
        [StringLength(15)]
        public string? AlternateMobileNumber { get; set; } = "-";
        public string? Gender { get; set; }
    }
}
