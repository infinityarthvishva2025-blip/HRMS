using System;

namespace HRMS.Models
{
    public class Attendance
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string? CheckoutStatus { get; set; }
        public Employee Employee { get; set; }
        public bool IsLate { get; set; }
        public bool IsEarlyLeave { get; set; }
        public double WorkingHours { get; set; }

    }
}
