namespace HRMS.Models.ViewModels
{
    public class GurukulProgressReportViewModel
    {
        public string EmployeeName { get; set; }
        public string EmployeeCode { get; set; }
        public int TotalVideos { get; set; }
        public int CompletedVideos { get; set; }
        public int ProgressPercentage { get; set; }
        public List<VideoProgressDetail> Details { get; set; } = new();
    }

    public class VideoProgressDetail
    {
        public string Title { get; set; }
        public string Category { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedOn { get; set; }
    }
}
