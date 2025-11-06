using System;

namespace HRMS.Models
{
    public class Payroll
    {
        public int Id { get; set; }
        public string EmployeeName { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal HRA { get; set; }
        public decimal TA { get; set; }
        public decimal Bonus { get; set; }
        public decimal Deductions { get; set; }

        public decimal NetSalary
        {
            get
            {
                return (BasicSalary + HRA + TA + Bonus) - Deductions;
            }
        }
    }

    public class InvestmentDeclaration
    {
        public decimal Section80C { get; set; }
        public decimal Section80D { get; set; }
        public decimal HomeLoanInterest { get; set; }
    }
}
