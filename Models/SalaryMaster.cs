public class SalaryMaster
{
    public int Id { get; set; }

    // Basic employee info
    public string EmployeeCode { get; set; }
    public string EmployeeName { get; set; }
    public string Designation { get; set; }
    public DateTime? DateOfJoining { get; set; }

    // Earnings
    public decimal Basic { get; set; }
    public decimal HRA { get; set; }
    public decimal Medical { get; set; }
    public decimal Conveyance { get; set; }
    public decimal SpecialAllowance { get; set; }
    public decimal OtherAllowance { get; set; }

    // Deductions
    public decimal PF { get; set; }
    public decimal ESI { get; set; }
    public decimal PT { get; set; }
    public decimal TDS { get; set; }

    // Computed salary
    public decimal GrossSalary { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetSalary { get; set; }

    // Bank details
    public string BankName { get; set; }
    public string AccountNumber { get; set; }
    public string IFSC { get; set; }
}
