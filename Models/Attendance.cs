using System;
using System.ComponentModel.DataAnnotations;

namespace HRMS.Models
{
    public class Attendance
    {
        public string Emp_Code { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public TimeSpan? InTime { get; set; }
        public TimeSpan? OutTime { get; set; }
        public decimal? Total_Hours { get; set; }

        // These two properties DO NOT exist in DB but we need them for late logic
        public bool IsLate { get; set; }
        public int LateMinutes { get; set; }
    }

}
