using System;

namespace HRMS.Models.ViewModels
{
    public class AttendanceRecordViewModel
    {
        public string EmpCode { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public DateTime? InTime { get; set; }
        public DateTime? OutTime { get; set; }
        public TimeSpan? TotalHours { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }

        public int Year { get; set; }
        public int Month { get; set; }

        public List<Attendance> Records { get; set; }
    }
}
