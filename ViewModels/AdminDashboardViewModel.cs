using HRMS.Models;

namespace HRMS.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalEmployees { get; set; }
        public int ActiveEmployees { get; set; }
        public int NewHires { get; set; }
        public List<Employee> LatestEmployees { get; set; }
        public Dictionary<string, int> DepartmentCount { get; set; }
    }

}
