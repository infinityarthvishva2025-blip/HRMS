namespace HRMS.Models
{ 
public class Payroll
{
    public int Id { get; set; }

    public string emp_code { get; set; }
    public string name { get; set; }

    public int working_days { get; set; }
    public decimal? leaves_taken { get; set; }
    public int? late_marks { get; set; }

    public decimal base_salary { get; set; }
    public decimal paid_days { get; set; }
    public decimal per_day_salary { get; set; }
    public decimal gross_salary { get; set; }
    public decimal total_deduction { get; set; }
    public decimal net_salary { get; set; }
    public decimal? total_pay { get; set; }

    // ✅ MUST BE INT (matches DB)
    public int month { get; set; }
    public int year { get; set; }
}
}


//public class Payroll
//{
//    public int Id { get; set; }
//    public string emp_code { get; set; }
//    public string name { get; set; }
//    public int? working_days { get; set; }
//    public decimal? leaves_taken { get; set; }
//    public int? late_marks { get; set; }
//    public int? late_gt_3 { get; set; }
//    public decimal? late_deduction_days { get; set; }
//    public decimal? day_presented { get; set; }
//    public decimal? base_salary { get; set; }
//    public decimal? perf_allowance { get; set; }
//    public decimal? other_allowance { get; set; }
//    public decimal? petrol_allowance { get; set; }
//    public decimal? reimbursement { get; set; }
//    public decimal? employee_ctc { get; set; }
//    public decimal? gross_salary { get; set; }
//    public decimal? prof_tax { get; set; }
//    public decimal? paid_days { get; set; }
//    public decimal? per_day_salary { get; set; }
//    public decimal? net_salary { get; set; }
//    public decimal? total_deduction { get; set; }
//    public decimal? total_pay { get; set; }
//    public int month { get; set; }
//    public int Year { get; set; }
//}
