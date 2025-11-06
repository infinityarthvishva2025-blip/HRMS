using HRMS.Models;

namespace HRMS.Services
{
    public interface IAttendanceService
    
    {
        IEnumerable<AttendanceRecord> GetRecentAttendance();
        bool IsJioTagWithinRange(double lat, double lng);
        AttendanceRecord MarkAttendance(string jioTag, double lat, double lng);
    }
}
