using HRMS.Models;
using System.Collections.Generic;



namespace HRMS.ViewModels
{
    public class AttendanceDashboardViewModel
    {
        public IEnumerable<AttendanceRecord> RecentRecords { get; set; }
    }
}
