namespace HRMS.Models.ViewModels
{
    public class GurukulProgressReportViewModel
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeCode { get; set; }

        // Totals
        public int TotalVideos { get; set; }
        public int CompletedVideos { get; set; }

        // Auto Calculated Progress %
        private int _progressPercentage;

        public int ProgressPercentage
        {
            get
            {
                if (TotalVideos == 0) return 0;
                return (CompletedVideos * 100) / TotalVideos;
            }
            set
            {
                _progressPercentage = value; // Allows controller assigning
            }
        }

        // List of all videos with progress
        public List<VideoProgressDetail> Details { get; set; } = new();

        // Helper (if needed in controller)
        public void CalculateProgress()
        {
            CompletedVideos = Details.Count(d => d.IsCompleted);
            TotalVideos = Details.Count;
        }
    }

    public class VideoProgressDetail
    {
        public int VideoId { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedOn { get; set; }
        public string VideoUrl { get; set; }
    }
}
