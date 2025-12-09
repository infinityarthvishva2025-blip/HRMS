namespace HRMS.Models
{
    public class CalendarViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; }
        public List<DateTime> Dates { get; set; } = new();
        public Dictionary<DateTime, List<Attendance>> DailyAttendance { get; set; } = new();
        public List<Employee> Employees { get; set; } = new();
        public int TotalEmployees { get; set; }
        public string ViewMode { get; set; } = "all";
        public string SelectedDepartment { get; set; } // Changed to string
        public int ItemPerPage { get; set; } = 10;
        public List<string> Departments { get; set; } // List of distinct department names
        
        public Dictionary<string, int> DepartmentCounts { get; set; } // Department name -> count
    }
}
