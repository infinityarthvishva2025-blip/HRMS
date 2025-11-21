using System;

namespace HRMS.Models.ViewModels
{
    public class PayrollSummaryVm
    {
        public string EmpCode { get; set; }
        public string EmpName { get; set; }

        public int Year { get; set; }
        public int Month { get; set; }

        public int TotalDaysInMonth { get; set; }
        public int PresentFullDays { get; set; }
        public int PresentHalfDays { get; set; }
        public int WeeklyOffDays { get; set; }
        public int AbsentDays { get; set; }

        public int LateMarks { get; set; }
        public int LateMarksOver3 { get; set; }
        public decimal LateDeductionDays { get; set; }

        public decimal PaidDays { get; set; }

        public decimal MonthlySalary { get; set; }
        public decimal PerDaySalary { get; set; }
        public decimal GrossSalary { get; set; }

        public decimal PerformanceAllowance { get; set; }
        public decimal OtherAllowances { get; set; }
        public decimal PetrolAllowance { get; set; }
        public decimal Reimbursement { get; set; }

        public decimal ProfessionalTax { get; set; }
        public decimal TotalDeductions { get; set; }

        public decimal NetSalary { get; set; }
    }
}
