using System;
using System.ComponentModel.DataAnnotations;

namespace HRMS.Models
{
    public class Payroll
    {
        [Key]
        public int Id { get; set; }

        // ================= EMPLOYEE DETAILS =================
        [Required]
        public string EmployeeCode { get; set; }

        [Required]
        public string EmployeeName { get; set; }

        // Stored as "04-2025"
        [Required]
        public string MonthYear { get; set; }

        // ================= ATTENDANCE DETAILS =================
        public int TotalWorkingDays { get; set; }
        public int DaysAttended { get; set; }
        public int TotalLeavesTaken { get; set; }

        public int LateMarks { get; set; }
        public int LateMarksOver3 { get; set; }

        // EXCEL shows float values like 2.5, 3.5 etc
        public decimal LateDeductionDays { get; set; }

        public decimal PaidDays { get; set; }

        // ================= SALARY + ALLOWANCES =================
        public decimal BaseSalary { get; set; }

        public decimal PerformanceAllowance { get; set; }
        public decimal OtherAllowances { get; set; }
        public decimal PetrolAllowance { get; set; }
        public decimal Reimbursement { get; set; }

        // ================= DEDUCTIONS =================
        public decimal ProfessionalTax { get; set; }

        public decimal TotalEarning { get; set; }        // gross before deductions
        public decimal TotalDeductions { get; set; }

        public decimal NetPay { get; set; }

        // ================= SYSTEM FIELDS =================
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
