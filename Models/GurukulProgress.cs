using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.Models
{
    public class GurukulProgress
    {
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public int VideoId { get; set; }

        public bool IsCompleted { get; set; } = false;

        public DateTime? CompletedOn { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; }

        [ForeignKey("VideoId")]
        public GurukulVideo Video { get; set; }
    }
}
