using System;

namespace HRMS.Models.ViewModels
{
    public class PayrollSummaryVm
    {
        public string EmpCode { get; set; } = string.Empty;
        public string EmpName { get; set; } = string.Empty;
        public string? FatherName { get; set; }
        public string? MotherName { get; set; }
        public DateTime? DOB_Date { get; set; }

        public int Year { get; set; }
        public int Month { get; set; }

        public int TotalDaysInMonth { get; set; }

        public decimal PresentHalfDays { get; set; }
        public decimal WeeklyOffDays { get; set; }
        public decimal AbsentDays { get; set; }

        public int LateMarks { get; set; }
        public decimal LateDeductionDays { get; set; }
        public decimal PaidDays { get; set; }

        public decimal MonthlySalary { get; set; }
        public decimal PerDaySalary { get; set; }
        public decimal? GrossSalary { get; set; }

        public decimal? PerformanceAllowance { get; set; }
        public decimal? OtherAllowances { get; set; }
        public decimal? PetrolAllowance { get; set; }
        public decimal? Reimbursement { get; set; }

        public decimal? ProfessionalTax { get; set; }
        public decimal? TotalDeductions { get; set; }
        public decimal? NetSalary { get; set; }

        //Bank details
        public string Department { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string IFSCCode { get; set; } = string.Empty;
        public string BankBranch { get; set; } = string.Empty;

    }
}
