using System;
using System.Collections.Generic;
using HRMS.Models;

namespace HRMS.Models.ViewModels
{
    public class EmployeeDashboardViewModel
    {
        // EMPLOYEE BASIC DETAILS
        public Employee Employee { get; set; }
        public string EmployeeName { get; set; }
        // Show on cards
        public string Department { get; set; }
        public string Position { get; set; }

        // ATTENDANCE SUMMARY
        public int TotalAttendance { get; set; }
        public string TodayStatus { get; set; }

        // LEAVE SUMMARY
        public int TotalLeaves { get; set; }
        public int ApprovedLeaves { get; set; }
        public int TotalLeave { get; set; }

        // UPCOMING
        public List<Employee> UpcomingBirthdays { get; set; }

        public int MonthWorkingDays { get; set; }
        public int MonthPresentDays { get; set; }
        public int MonthAttendancePercent { get; set; }

        public int SelectedMonth { get; set; }
        public int SelectedYear { get; set; }
        public int WorkingDays { get; set; }


        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int LeaveDays { get; set; }
        public int HolidayDays { get; set; }

        public List<Employee> TodaysBirthdays { get; set; } = new();
        public List<Employee> TomorrowsBirthdays { get; set; } = new();


    }
}
