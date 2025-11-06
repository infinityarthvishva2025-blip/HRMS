using System;
using System.ComponentModel.DataAnnotations;

namespace HRMS.Models
{
    public class Expense
    {
        public int Id { get; set; }
        public string? EmployeeName { get; set; }
        public string? ExpenseType { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; } = "Pending";
    }

}
