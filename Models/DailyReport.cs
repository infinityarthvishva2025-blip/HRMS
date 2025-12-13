using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HRMS.Models
{
    public class DailyReport
    {
        [Key]   // ADD THIS LINE ✔
        public int ReportId { get; set; }

        public int SenderId { get; set; }

        public string TodaysWork { get; set; }
        public string PendingWork { get; set; }
        public string Issues { get; set; }

        // stored file name (e.g. 1f2e3a4b.pdf)
        public string? AttachmentPath { get; set; }

        public DateTime CreatedDate { get; set; }

        // navigation
        public virtual Employee Sender { get; set; }
        public virtual ICollection<DailyReportRecipient> Recipients { get; set; }
    }
}
