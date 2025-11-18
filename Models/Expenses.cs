using System;
using System.ComponentModel.DataAnnotations;

namespace HRMS.Models
{
    public class Expenses
    {
        public int Id { get; set; }

        [Required]
        public string ExpenseType { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public string? Description { get; set; }

        public string? ProofFilePath { get; set; }

        [Required]
        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        public string Status { get; set; } = "Pending";

        public string? HRComment { get; set; }
    }
}
