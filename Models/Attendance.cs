using System;
using System.ComponentModel.DataAnnotations;

namespace HRMS.Models
{
    public class Attendance
    {
        [Key]
        public string EmpCode { get; set; }     // VARCHAR in DB
        public DateTime Date { get; set; }       // Date column
        public string Status { get; set; }       // P / A / WO / etc.
        public DateTime? InTime { get; set; }    // nullable datetime
        public DateTime? OutTime { get; set; }   // nullable datetime
        public TimeSpan? TotalHours { get; set; } // time(7) column
    }
}
