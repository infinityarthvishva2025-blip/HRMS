using HRMS.ViewModels;

namespace HRMS.Models
{
    public class Attendance : AttendanceDashboardViewModel


    {
        public int Id { get; set; }
        public string? EmployeeName { get; set; }
        public string? Method { get; set; }
        public string? Status { get; set; }
        public string? JioTag { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan? InTime { get; set; }
        public TimeSpan? OutTime { get; set; }
        public string? Location { get; set; }
    }
}
