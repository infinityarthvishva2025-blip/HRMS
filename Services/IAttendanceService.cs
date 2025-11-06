using HRMS.Models;

namespace HRMS.Services
{
    public interface IAttendanceService
    
    {
        IEnumerable<Attendance> GetRecentAttendance();
        bool IsJioTagWithinRange(double lat, double lng);
        Attendance MarkAttendance(string jioTag, double lat, double lng);
    }
}
