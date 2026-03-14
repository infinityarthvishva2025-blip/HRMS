using System;

namespace HRMS.Models
{
    public class GurukulProgress
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int VideoId { get; set; }

        public bool IsCompleted { get; set; } = false;
        public DateTime? CompletedOn { get; set; }

        // navigation props (optional)
        public Employee? Employee { get; set; }
        public GurukulVideo? Video { get; set; }
    }
}
