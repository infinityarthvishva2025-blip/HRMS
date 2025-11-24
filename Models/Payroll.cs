using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Payroll")]
public class Payroll
{
    [Key]
    public int Id { get; set; }

    [Column("emp_code")]
    public string EmployeeCode { get; set; }

    [Column("name")]
    public string Name { get; set; }

    [Column("working_days")]
    public int WorkingDays { get; set; }

    [Column("leaves_taken")]
    public decimal LeavesTaken { get; set; }

    [Column("late_marks")]
    public int LateMarks { get; set; }

    [Column("late_gt_3")]
    public int LateGt3 { get; set; }

    [Column("late_deduction_days")]
    public decimal LateDeductionDays { get; set; }

    [Column("day_presented")]
    public decimal PaidDays { get; set; }

    [Column("base_salary")]
    public decimal BaseSalary { get; set; }

    [Column("perf_allowance")]
    public decimal? PerfAllowance { get; set; }

    [Column("other_allowance")]
    public decimal? OtherAllowance { get; set; }

    [Column("petrol_allowance")]
    public decimal? PetrolAllowance { get; set; }

    [Column("reimbursement")]
    public decimal? Reimbursement { get; set; }

    [Column("employee_ctc")]
    public decimal? EmployeeCTC { get; set; }

    [Column("gross_salary")]
    public decimal? GrossSalary { get; set; }

    [Column("prof_tax")]
    public decimal? ProfTax { get; set; }

    [Column("paid_days")]
    public decimal? PaidDaysDecimal { get; set; }

    [Column("per_day_salary")]
    public decimal? PerDaySalary { get; set; }

    [Column("net_salary")]
    public decimal? NetSalary { get; set; }

    [Column("total_deduction")]
    public decimal? TotalDeduction { get; set; }

    [Column("total_pay")]
    public decimal? TotalPay { get; set; }

    [Column("month")]
    public string Month { get; set; }
}
