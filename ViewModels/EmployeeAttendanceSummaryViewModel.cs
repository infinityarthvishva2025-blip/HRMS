using System;
using System.Collections.Generic;
using HRMS.Models;

namespace HRMS.Models.ViewModels
{
    public class EmployeeAttendanceSummaryViewModel
    {
        public Employee Employee { get; set; }
        public List<Attendance> AttendanceRecords { get; set; }

        public int TotalDays { get; set; }
        public int TotalLateDays { get; set; }
        public int TotalEarlyLeaveDays { get; set; }
        public string AverageWorkingHours { get; set; }


        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}
