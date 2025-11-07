using System.Collections.Generic;
using HRMS.Models;

namespace HRMS.Repositories.Interfaces
{
    //public interface IAttendanceRepository
    //{
    //    IEnumerable<Attendance> GetAll();   // ✅ NO BODY
    //    void Add(Attendance record);        // ✅ NO BODY
    //}


public interface IAttendanceRepository
{
    Task<List<AttendanceRecord>> GetRecentAttendanceRecordsAsync(int days = 7);
    Task<List<AttendanceRecord>> GetTodaysAttendanceAsync();
    Task<AttendanceRecord?> MarkAttendanceAsync(GeoTagRequest request);
    Task<List<Employee>> GetEmployeesWithGeoTagAsync();
    Task<bool> IsWithinRangeAsync(double lat1, double lon1, double lat2, double lon2, double maxDistanceKm = 100);
    Task<OfficeLocation> GetOfficeLocationAsync();
}
}
