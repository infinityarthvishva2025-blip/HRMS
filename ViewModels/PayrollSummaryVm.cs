using System;

namespace HRMS.Models.ViewModels
{
    public class PayrollSummaryVm
    {
        // Employee
        public string EmpCode { get; set; }
        public string EmpName { get; set; }

        // Period
        public int Year { get; set; }
        public int Month { get; set; }

        // Attendance
        public int TotalDaysInMonth { get; set; }
        public decimal AbsentDays { get; set; }
        public int LateMarks { get; set; }
        public decimal LateDeductionDays { get; set; }
        public decimal PaidDays { get; set; }
        public int TotalSaturdayPaid { get; set; }
        public decimal OtherDeductions { get; set; }

        // Salary Structure
        public decimal MonthlySalary { get; set; }
        public decimal PerDaySalary { get; set; }
        public decimal GrossSalary { get; set; }

        public decimal PerformanceAllowance { get; set; }
        public decimal OtherAllowances { get; set; }
        public decimal PetrolAllowance { get; set; }
        public decimal Reimbursement { get; set; }

        // Deductions
        public decimal ProfessionalTax { get; set; }
        public decimal TotalDeductions { get; set; }

        // Final Salary
        public decimal NetSalary { get; set; }
        public decimal TotalPay { get; set; }

        // Optional additional items to avoid errors
        public int PresentHalfDays { get; set; }
        public int WeeklyOffDays { get; set; }


        // Bank Details
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string IFSCCode { get; set; }
        public string BankBranch { get; set; }

        // Job details
        public string Department { get; set; }
        public string Designation { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}
