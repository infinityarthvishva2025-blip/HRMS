namespace HRMS.ViewModels
{
    public class AttendanceRecordVm
    {
        public string empCode { get; set; }
        public string Emp_Code { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }   // P, A, WO, WOP, L, AUTO

        public TimeSpan? InTime { get; set; }
        public TimeSpan? OutTime { get; set; }

        public bool CorrectionRequested { get; set; }
        public string CorrectionStatus { get; set; }
        public string TotalHours { get; set; } = "--";
        public string DisplayCheckIn =>
            InTime.HasValue ? DateTime.Today.Add(InTime.Value).ToString("hh:mm tt") : "--";

        public string DisplayCheckOut =>
            OutTime.HasValue ? DateTime.Today.Add(OutTime.Value).ToString("hh:mm tt") : "--";

        public string WorkingHours
        {
            get
            {
                if (InTime.HasValue && OutTime.HasValue)
                {
                    var diff = OutTime.Value - InTime.Value;
                    return $"{diff.Hours}h {diff.Minutes}m";
                }
                return "--";
            }
        }
    }
}
