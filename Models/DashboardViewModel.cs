

namespace HRMS.Models
{
    public class DashboardViewModel
    {
        public int TotalEmployees { get; set; }
        public int TodaysPresent { get; set; }
        public int GeoTagsAssigned { get; set; }
        public int PendingLeaves { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}