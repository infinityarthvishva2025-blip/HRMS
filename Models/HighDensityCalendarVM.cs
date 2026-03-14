
using HRMS.Models;
using System;
using System.Collections.Generic;

namespace HRMS.Models.ViewModels
{
    public class HighDensityCalendarVM
    {
        public int Year { get; set; }
        public int Month { get; set; }

        public List<Employee> Employees { get; set; }
        public List<Attendance> Attendance { get; set; }
   

      

        // Filters
        public string? Search { get; set; }
        public string? Department { get; set; }
        public List<string> Departments { get; set; } = new();

        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages { get; set; }
        public int TotalEmployees { get; set; }

        // Data
     
    }
}
