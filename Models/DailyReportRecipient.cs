using System;
using System.ComponentModel.DataAnnotations;

namespace HRMS.Models
{
    public class DailyReportRecipient
    {
        [Key]   // ADD THIS LINE ✔
        public int Id { get; set; }

        public int ReportId { get; set; }
        public int ReceiverId { get; set; }

        public bool IsRead { get; set; }
        public DateTime? ReadDate { get; set; }

        // navigation
        public virtual DailyReport Report { get; set; }
        public virtual Employee Receiver { get; set; }
    }
}
