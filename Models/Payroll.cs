using System;
using System.ComponentModel.DataAnnotations;

namespace HRMS.Models
{
    public class Payroll
    {
        [Key]
        public int Id { get; set; }

        // ---------------- EMPLOYEE DETAILS ----------------
        [Required(ErrorMessage = "Employee name is required")]
        [Display(Name = "Employee Name")]
        public string EmployeeName { get; set; }

        [Required(ErrorMessage = "Employee code is required")]
        [Display(Name = "Employee Code")]
        public string EmployeeCode { get; set; }

        [Display(Name = "Designation")]
        public string? Designation { get; set; }

        [Display(Name = "PAN Number")]
        [StringLength(10, ErrorMessage = "PAN number must be 10 characters")]
        public string? PAN { get; set; }

        [Display(Name = "Bank Account Number")]
        [Required(ErrorMessage = "Bank account number is required")]
        [StringLength(20, ErrorMessage = "Bank account number cannot exceed 20 digits")]
        public string BankAccountNumber { get; set; }

        [Display(Name = "Bank Name")]
        public string? BankName { get; set; }

        [Required(ErrorMessage = "Date of joining is required")]
        [Display(Name = "Date of Joining")]
        public DateTime DateOfJoining { get; set; }

        [Required(ErrorMessage = "Month and Year are required")]
        [Display(Name = "Salary Month & Year")]
        public string MonthYear { get; set; }



        // ---------------- ATTENDANCE ----------------
        [Required(ErrorMessage = "Total working days are required")]
        [Range(1, 31, ErrorMessage = "Working days must be between 1 and 31")]
        [Display(Name = "Total Working Days")]
        public int TotalWorkingDays { get; set; }

        [Required(ErrorMessage = "Days attended are required")]
        [Range(0, 31, ErrorMessage = "Days attended must be between 0 and 31")]
        [Display(Name = "Days Attended")]
        public int DaysAttended { get; set; }

        [Range(0, 31, ErrorMessage = "Leaves taken must be between 0 and 31")]
        [Display(Name = "Total Leaves Taken")]
        public int TotalLeavesTaken { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Deduction for leaves cannot be negative")]
        [Display(Name = "Deduction for Leaves")]
        public decimal DeductionForLeaves { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Deduction for late marks cannot be negative")]
        [Display(Name = "Deduction for Late Marks")]
        public decimal DeductionForLateMarks { get; set; }


        // ---------------- EARNINGS ----------------
        [Required(ErrorMessage = "Base salary is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Base salary cannot be negative")]
        [Display(Name = "Base Salary")]
        public decimal BaseSalary { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Performance allowance cannot be negative")]
        [Display(Name = "Performance Allowance")]
        public decimal PerformanceAllowance { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Other allowances cannot be negative")]
        [Display(Name = "Other Allowances")]
        public decimal OtherAllowances { get; set; }


        // ---------------- DEDUCTIONS ----------------
        [Range(0, double.MaxValue, ErrorMessage = "Professional tax cannot be negative")]
        [Display(Name = "Professional Tax")]
        public decimal ProfessionalTax { get; set; }


        // ---------------- TOTALS ----------------
        [Display(Name = "Total Earnings")]
        [Range(0, double.MaxValue)]
        public decimal TotalEarning { get; set; }

        [Display(Name = "Total Deductions")]
        [Range(0, double.MaxValue)]
        public decimal TotalDeductions { get; set; }

        [Display(Name = "Net Pay")]
        [Range(0, double.MaxValue)]
        public decimal NetPay { get; set; }


        // ---------------- SYSTEM FIELDS ----------------
        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
