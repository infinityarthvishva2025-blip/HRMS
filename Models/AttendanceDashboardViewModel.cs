using HRMS.Models;
using System.Collections.Generic;



namespace HRMS.ViewModels
{
    public class AttendanceDashboardViewModel
    {
        public IEnumerable<Attendance>? RecentRecords { get; set; }
    }
}
