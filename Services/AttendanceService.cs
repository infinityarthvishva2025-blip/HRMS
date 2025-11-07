using HRMS.Models;
using HRMS.Services;

namespace HRMS.Services
{
   


    public class AttendanceService 
    {
        private readonly List<Attendance> _attendanceRecords;

        private readonly double officeLat = 19.0760;
        private readonly double officeLng = 72.8777;

        public AttendanceService()
        {
            _attendanceRecords = new List<Attendance>
            {
                //new Attendance { Id=1, Date=DateTime.Today, EmployeeName="John Smith", InTime=new TimeSpan(9,05,0), OutTime=new TimeSpan(18,15,0), Method="Jio Tag", Status="Present", Location="19.0761, 72.8778" },
                //new Attendance { Id=2, Date=DateTime.Today, EmployeeName="Sarah Johnson", InTime=new TimeSpan(9,15,0), OutTime=new TimeSpan(17,45,0), Method="Face Auth", Status="Present", Location="19.0762, 72.8779" },
                //new Attendance { Id=3, Date=DateTime.Today, EmployeeName="Mike Wilson", InTime=new TimeSpan(8,55,0), OutTime=new TimeSpan(18,30,0), Method="Jio Tag", Status="Present", Location="19.0760, 72.8777" },
                //new Attendance { Id=4, Date=DateTime.Today, EmployeeName="Emily Brown", Status="On Leave" },
                //new Attendance { Id=5, Date=DateTime.Today, EmployeeName="David Lee", InTime=new TimeSpan(9,10,0), OutTime=new TimeSpan(18,20,0), Method="Jio Tag", Status="Present", Location="19.0763, 72.8776" }
            };
        }

        public IEnumerable<AttendanceService> GetRecentAttendance() => (IEnumerable<AttendanceService>)_attendanceRecords;

        public bool IsJioTagWithinRange(double lat, double lng)
        {
            double distance = Math.Sqrt(Math.Pow(lat - officeLat, 2) + Math.Pow(lng - officeLng, 2));
            return distance < 1;  // approx <100km radius
        }

        public Attendance MarkAttendance(string jioTag, double lat, double lng)
        {
            if (!IsJioTagWithinRange(lat, lng))
                return null;

            var rec = new Attendance
            {
                //Id = _attendanceRecords.Max(x => x.Id) + 1,
                //EmployeeName = $"JioTag User ({jioTag})",
                //Date = DateTime.Today,
                //InTime = DateTime.Now.TimeOfDay,
                //Method = "Jio Tag",
                //Status = "Present",
                //Location = $"{lat}, {lng}"
            };

            _attendanceRecords.Add(rec);
            return rec;
        }
    }

    public class Attendance
    {
    }
}
