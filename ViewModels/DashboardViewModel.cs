using HRMS.Models;

namespace HRMS.ViewModels
{
    public class DashboardViewModel
    {
        public Employee Employee { get; set; }
        public int TotalAttendance { get; set; }
        public int TotalLeave { get; set; }
        public List<Employee> UpcomingBirthdays { get; set; }
    }
}
