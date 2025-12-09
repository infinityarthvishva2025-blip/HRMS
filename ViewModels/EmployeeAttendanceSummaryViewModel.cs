using HRMS.Models;
using HRMS.ViewModels;
using System;
using System.Collections.Generic;

namespace HRMS.Models.ViewModels
{
    public class EmployeeAttendanceSummaryViewModel
    {
        public Employee Employee { get; set; }

        // The final correct attendance list
        public List<AttendanceRecordVm> AttendanceRecords { get; set; } = new List<AttendanceRecordVm>();

        public int TotalDays { get; set; }
        public int TotalLateDays { get; set; }
        public int TotalEarlyLeaveDays { get; set; }
        public string AverageWorkingHours { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}
