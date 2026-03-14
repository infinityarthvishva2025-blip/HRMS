using System;
namespace HRMS.Models
{
    public class Announcement
    {
        public int Id { get; set; }

        public string Title { get; set; }
        public string Message { get; set; }

        // Target system
        public bool IsGlobal { get; set; } = false;     // all employees
        public string? TargetDepartments { get; set; }  // comma list
        public string? TargetEmployees { get; set; }    // comma list (IDs)

        public bool IsUrgent { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        // Read tracking
        public string? ReadByEmployees { get; set; }    // comma list of employee IDs
    }
}