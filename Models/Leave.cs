using System;

namespace HRMS.Models
{
    public class Leave
    {
     
        public int Id { get; set; }   // EF will auto-detect this as an identity column

       
        public string LeaveType { get; set; }

        
        public DateTime StartDate { get; set; }

  
        public DateTime EndDate { get; set; }

      
        public string Reason { get; set; }

      
        public string Status { get; set; } = "Pending";
    }
}
