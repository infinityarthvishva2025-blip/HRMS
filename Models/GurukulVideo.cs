using System;

namespace HRMS.Models
{
    public class GurukulVideo
    {
        public int Id { get; set; }

        public string? TitleGroup { get; set; }
        public string? Category { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";

        // Video file or external URL
        public string VideoPath { get; set; } = "";
        public bool IsExternal { get; set; } = false;

        public string? ThumbnailPath { get; set; }
        public DateTime UploadedOn { get; set; } = DateTime.UtcNow;

        // =========================
        // ACCESS PERMISSIONS
        // =========================
        // If both are NULL  => visible to ALL employees
        // If AllowedDepartment != null, AllowedEmployeeId == null
        //      => visible to employees whose Employee.Department == AllowedDepartment
        // If AllowedEmployeeId != null
        //      => visible ONLY to that employee (ignores department)
        public string? AllowedDepartment { get; set; }      // department name (same text as Employee.Department)
        public int? AllowedEmployeeId { get; set; }         // specific employee

        // Navigation (optional – useful in HRList)
        public Employee? AllowedEmployee { get; set; }
    }
}
