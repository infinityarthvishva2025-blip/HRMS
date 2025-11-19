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
    }
}
