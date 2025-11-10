using System;
using System.ComponentModel.DataAnnotations;

namespace HRMS.Models
{
    public class Expenses
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Expense Type")]
        public string ExpenseType { get; set; }

        [Required]
        public string Description { get; set; }

        public string? ProofFilePath { get; set; }


        [Required]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        public string Status { get; set; } = "Pending";
    }
}
