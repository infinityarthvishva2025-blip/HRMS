

using System;
using System.ComponentModel.DataAnnotations;

namespace HRMS.Models
{
    public class Attendance
    {


        [Key]
        public int? Id { get; set; } // Should have identity key
       
        public string Emp_Code { get; set; }
    
        public DateTime Date { get; set; }
        public string Status { get; set; }


        public TimeSpan? InTime { get; set; }
        public TimeSpan? OutTime { get; set; }
        public decimal? Total_Hours { get; set; }


        //public decimal? Total_Hours { get; set; }

        public bool IsLate { get; set; }
        public int LateMinutes { get; set; }
        public DateTime? Att_Date { get; set; }
        public bool CorrectionRequested { get; set; } = false;
        public string? CorrectionProofPath { get; set; }

        public string? CorrectionRemark { get; set; }
        public string CorrectionStatus { get; set; } = "None"; // None, Pending, Approved, Rejected

        public DateTime? CorrectionRequestedOn { get; set; }
        public string? ReviewedBy { get; set; }
        public DateTime? ReviewedOn { get; set; }

        public bool? IsGeoAttendance { get; set; }

        public double? CheckInLatitude { get; set; }
        public double? CheckInLongitude { get; set; }

        public double? CheckOutLatitude { get; set; }
        public double? CheckOutLongitude { get; set; }
        public bool IsCompOffCredited { get; set; } = false;
        // ✅ ADD THESE TWO
        public string? RequestedByRole { get; set; }
        public string? PendingWithRole { get; set; }
    }

}
