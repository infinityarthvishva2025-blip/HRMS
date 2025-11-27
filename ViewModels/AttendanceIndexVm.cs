namespace HRMS.ViewModels 
{

    public class AttendanceIndexVm
    {
        public string Emp_Code { get; set; }
        public string EmpName { get; set; }
        public DateTime AttDate { get; set; }
        public string DayName => AttDate.ToString("ddd");
        public string Status { get; set; }
        public TimeSpan? InTime { get; set; }
        public TimeSpan? OutTime { get; set; }
        public string TotalHours { get; set; } = "--";
        public bool IsLate { get; set; }      // calculated, not from DB

        public string Date => AttDate.ToString("dd-mm-yyyy");
    }
}

