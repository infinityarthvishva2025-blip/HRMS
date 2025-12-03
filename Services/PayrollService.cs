using HRMS.Data;
using HRMS.Models;
using HRMS.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HRMS.Services
{
    public class PayrollService
    {
        private readonly ApplicationDbContext _context;

        public PayrollService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // PROFESSIONAL TAX (Maharashtra)
        // ============================================================
        public decimal CalculatePT(decimal grossSalary, string gender, int month)
        {
            gender = gender?.ToLower();

            if (month == 2)
                return 300m;

            if (gender == "female")
            {
                if (grossSalary <= 25000) return 0m;
                return 200m;
            }
            else
            {
                if (grossSalary <= 7500) return 0m;
                if (grossSalary <= 10000) return 175m;
                return 200m;
            }
        }

        // ============================================================
        // BUILD SUMMARY FOR ONE EMPLOYEE — MAIN FUNCTION
        // ============================================================
        //public PayrollSummaryVm BuildMonthlySummary(string empCode, int year, int month)
        //{
        //    var emp = _context.Employees.FirstOrDefault(x => x.EmployeeCode == empCode);
        //    if (emp == null) return null;

        //    string monthName = new DateTime(year, month, 1).ToString("MMMM").ToUpper();

        //    // Load payroll row IF EXISTS
        //    var payroll = _context.Payroll
        //        .FirstOrDefault(p => p.emp_code == empCode &&  p.month.ToUpper() == monthName);

        //    decimal baseSalary = payroll?.base_salary ?? emp.Salary ?? 0;

        //    // ============================================================
        //    // GET ATTENDANCE FOR THE MONTH
        //    // ============================================================
        //    var start = new DateTime(year, month, 1);
        //    var end = start.AddMonths(1).AddDays(-1);

        //    var att = _context.Attendances
        //        .Where(a => a.Emp_Code == empCode && a.Date >= start && a.Date <= end)
        //        .ToList();

        //    int totalDaysInMonth = DateTime.DaysInMonth(year, month);

        //    if (!att.Any())
        //        return null; // no attendance → no payroll

        //    // Normalize Status
        //    string clean(string s) => (s ?? "").Trim().ToUpper();

        //    // PRESENTS
        //    int presentFull = att.Count(a => clean(a.Status) == "P");
        //    int presentHalf = att.Count(a => clean(a.Status) == "½P" || clean(a.Status) == "P½");

        //    // WEEKLY OFF (PAID)
        //    int weekOff = att.Count(a => clean(a.Status) == "WO" || clean(a.Status) == "W/O");

        //    // SATURDAY (PAID)
        //    int saturdayPaid = att.Count(a => clean(a.Status) == "WOP");

        //    // LEAVES
        //    int paidLeave = att.Count(a => clean(a.Status) == "PL" || clean(a.Status) == "CL" || clean(a.Status) == "SL");
        //    int unpaidLeave = att.Count(a => clean(a.Status) == "L");
        //    int absent = att.Count(a => clean(a.Status) == "A");

        //    // ============================
        //    // LATE MARK DEDUCTIONS
        //    // ============================
        //    int totalLateMarks = payroll?.late_marks ?? 0;
        //    decimal lateDed = 0m;

        //    if (totalLateMarks > 3)
        //        lateDed = (totalLateMarks - 3) * 0.5m;

        //    // ============================
        //    // CALCULATE PAID DAYS
        //    // ============================
        //    decimal paidDays =
        //        presentFull +
        //        (presentHalf * 0.5m) +
        //        paidLeave +
        //        weekOff +
        //        saturdayPaid -
        //        lateDed;

        //    if (paidDays < 0) paidDays = 0;

        //    // ============================
        //    // SALARY CALCULATIONS
        //    // ============================
        //    decimal perDaySalary = baseSalary / totalDaysInMonth;
        //    decimal grossSalary = perDaySalary * paidDays;

        //    string gender = emp.Gender?.ToLower() ?? "male";
        //    decimal pt = CalculatePT(grossSalary, gender, month);

        //    decimal otherDeductions = payroll?.total_deduction ?? 0;
        //    decimal totalDeductions = pt + otherDeductions;
        //    decimal netSalary = grossSalary - totalDeductions;

        //    // ============================================================
        //    // RETURN MODEL
        //    // ============================================================
        //    return new PayrollSummaryVm
        //    {
        //        EmpCode = empCode,
        //        EmpName = emp.Name,
        //        Department = emp.Department,
        //        Designation = emp.Position,

        //        Year = year,
        //        Month = month,
        //        TotalDaysInMonth = totalDaysInMonth,

        //        PresentHalfDays = presentHalf,
        //        WeeklyOffDays = weekOff,
        //        TotalSaturdayPaid = saturdayPaid,
        //        AbsentDays = absent + unpaidLeave, // attendance based

        //        LateMarks = totalLateMarks,
        //        LateDeductionDays = lateDed,

        //        PaidDays = paidDays,

        //        MonthlySalary = baseSalary,
        //        PerDaySalary = perDaySalary,
        //        GrossSalary = grossSalary,

        //        PerformanceAllowance = payroll?.perf_allowance ?? 0,
        //        OtherAllowances = payroll?.other_allowance ?? 0,
        //        PetrolAllowance = payroll?.petrol_allowance ?? 0,
        //        Reimbursement = payroll?.reimbursement ?? 0,

        //        ProfessionalTax = pt,
        //        OtherDeductions = otherDeductions,
        //        TotalDeductions = totalDeductions,
        //        NetSalary = netSalary,
        //        TotalPay = netSalary,

        //        BankName = emp.BankName,
        //        AccountNumber = emp.AccountNumber,
        //        IFSCCode = emp.IFSC,
        //        BankBranch = emp.Branch
        //    };
        //}

        // ============================================================
        // MONTHLY SUMMARY LIST (USED BY CONTROLLER)
        // ============================================================
        public List<PayrollSummaryVm> GetMonthlySummaries(int year, int month)
        {
            var employees = _context.Employees
    .Select(e => e.EmployeeCode)
    .ToList(); 

            List<PayrollSummaryVm> list = new();

            foreach (var code in employees)
            {
                var vm = BuildMonthlySummary(code, year, month);
                if (vm != null)
                    list.Add(vm);
            }

            return list.OrderBy(x => x.EmpName).ToList();
        }

        public PayrollSummaryVm BuildMonthlySummary(string empCode, int year, int month)
        {
            var emp = _context.Employees.FirstOrDefault(x => x.EmployeeCode == empCode);
            if (emp == null) return null;

            // Correct month name for display only
            string monthName = new DateTime(year, month, 1).ToString("MMMM").ToUpper();

            // ============================================================
            // LOAD PAYROLL ROW USING NUMERIC MONTH + YEAR  🔥 FIXED HERE
            // ============================================================
            var payroll = _context.Payroll
                .FirstOrDefault(p => p.emp_code == empCode &&
                                     p.month == month &&   // <-- FIXED (removed ToUpper)
                                     p.Year == year);      // <-- added year filter

            decimal baseSalary = payroll?.base_salary ?? emp.Salary ?? 0;

            // ============================================================
            // GET ATTENDANCE FOR THE MONTH
            // ============================================================
            var start = new DateTime(year, month, 1);
            var end = start.AddMonths(1).AddDays(-1);

            var att = _context.Attendances
                .Where(a => a.Emp_Code == empCode && a.Date >= start && a.Date <= end)
                .ToList();

            int totalDaysInMonth = DateTime.DaysInMonth(year, month);

            if (!att.Any())
                return null; // No attendance → no payroll

            // Normalize function
            string clean(string s) => (s ?? "").Trim().ToUpper();

            // PRESENTS
            int presentFull = att.Count(a => clean(a.Status) == "P");
            int presentHalf = att.Count(a => clean(a.Status) == "½P" || clean(a.Status) == "P½");

            // WEEKLY OFF (PAID)
            int weekOff = att.Count(a => clean(a.Status) == "WO" || clean(a.Status) == "W/O");

            // SATURDAY (PAID)
            int saturdayPaid = att.Count(a => clean(a.Status) == "WOP");

            // LEAVES
            int paidLeave = att.Count(a => clean(a.Status) == "PL" || clean(a.Status) == "CL" || clean(a.Status) == "SL");
            int unpaidLeave = att.Count(a => clean(a.Status) == "L");
            int absent = att.Count(a => clean(a.Status) == "A");

            // ============================
            // LATE MARK DEDUCTIONS
            // ============================
            int totalLateMarks = payroll?.late_marks ?? 0;
            decimal lateDed = 0m;

            if (totalLateMarks > 3)
                lateDed = (totalLateMarks - 3) * 0.5m;

            // ============================
            // CALCULATE PAID DAYS
            // ============================
            decimal paidDays =
                presentFull +
                (presentHalf * 0.5m) +
                paidLeave +
                weekOff +
                saturdayPaid -
                lateDed;

            if (paidDays < 0) paidDays = 0;

            // ============================
            // SALARY CALCULATIONS
            // ============================
            decimal perDaySalary = baseSalary / totalDaysInMonth;
            decimal grossSalary = perDaySalary * paidDays;

            string gender = emp.Gender?.ToLower() ?? "male";
            decimal pt = CalculatePT(grossSalary, gender, month);

            decimal otherDeductions = payroll?.total_deduction ?? 0;
            decimal totalDeductions = pt + otherDeductions;
            decimal netSalary = grossSalary - totalDeductions;

            // ============================================================
            // RETURN MODEL
            // ============================================================
            return new PayrollSummaryVm
            {
                EmpCode = empCode,
                EmpName = emp.Name,
                Department = emp.Department,
                Designation = emp.Position,

                Year = year,
                Month = month,
                TotalDaysInMonth = totalDaysInMonth,

                PresentHalfDays = presentHalf,
                WeeklyOffDays = weekOff,
                TotalSaturdayPaid = saturdayPaid,
                AbsentDays = absent + unpaidLeave,

                LateMarks = totalLateMarks,
                LateDeductionDays = lateDed,

                PaidDays = paidDays,

                MonthlySalary = baseSalary,
                PerDaySalary = perDaySalary,
                GrossSalary = grossSalary,

                PerformanceAllowance = payroll?.perf_allowance ?? 0,
                OtherAllowances = payroll?.other_allowance ?? 0,
                PetrolAllowance = payroll?.petrol_allowance ?? 0,
                Reimbursement = payroll?.reimbursement ?? 0,

                ProfessionalTax = pt,
                OtherDeductions = otherDeductions,
                TotalDeductions = totalDeductions,
                NetSalary = netSalary,
                TotalPay = netSalary,

                BankName = emp.BankName,
                AccountNumber = emp.AccountNumber,
                IFSCCode = emp.IFSC,
                BankBranch = emp.Branch
            };
        }

    }
}
