//using System;
//using System.ComponentModel.DataAnnotations;

//namespace HRMS.Models
//{
//    public class Attendance
//    {
//        [Key]
//        public int Id { get; set; }  // Primary key

//        public string Emp_Code { get; set; }
//        public DateTime Date { get; set; }
//        public string Status { get; set; } = "-";

//        public TimeSpan? InTime { get; set; } = TimeSpan.Zero;
//        public TimeSpan? OutTime { get; set; } = TimeSpan.Zero;
//        public decimal? Total_Hours { get; set; } = 0.0m;

//        public bool IsLate { get; set; } = false ;
//        public int LateMinutes { get; set; } = 0;
//        public DateTime? Att_Date { get; set; } = DateTime.Now;
//        public bool CorrectionRequested { get; set; } = false;
//        public string? CorrectionRemark { get; set; } = "NONE";
//        public string CorrectionStatus { get; set; } = "None";
//    }
//}


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
        public string? CorrectionRemark { get; set; }
        public string CorrectionStatus { get; set; } = "None"; // None, Pending, Approved, Rejected

        

    }

}
