namespace HRMS.Models.ViewModels
{
    public class EmployeeDashboardViewModel
    {
     
        public string EmployeeName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string ProfileImage { get; set; }
        public int TotalLeaves { get; set; }
        public int ApprovedLeaves { get; set; }
    }
}
