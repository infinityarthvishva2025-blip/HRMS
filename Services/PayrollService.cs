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

        // ============================
        // PROFESSIONAL TAX (MAHARASHTRA)
        // ============================
        public decimal CalculatePT(decimal netSalary, string gender, int month)
        {
            gender = gender?.ToLower();

            // February special PT
            if (month == 2)
                return 300m;

            if (gender == "female")
            {
                if (netSalary <= 25000)
                    return 0m;
                return 200m;
            }
            else   // male
            {
                if (netSalary <= 7500)
                    return 0m;
                else if (netSalary <= 10000)
                    return 175m;
                return 200m;
            }
        }

        // ============================
        // BUILD SUMMARY FOR ONE EMPLOYEE
        // ============================
        public PayrollSummaryVm BuildMonthlySummary(string empCode, int year, int month)
        {
            string monthName = new DateTime(year, month, 1)
                .ToString("MMMM").ToUpper();

            var payroll = _context.Payroll
                .FirstOrDefault(p =>
                    p.emp_code == empCode &&
                    p.month.ToUpper() == monthName);

            if (payroll == null)
                return null;

            var emp = _context.Employees
                .FirstOrDefault(e => e.EmployeeCode == empCode);

            // ============================
            // FETCH ATTENDANCE FOR MONTH
            // ============================
            var start = new DateTime(year, month, 1);
            var end = start.AddMonths(1).AddDays(-1);

            var att = _context.Attendances
                .Where(a => a.Emp_Code == empCode &&
                            a.Date >= start &&
                            a.Date <= end)
                .ToList();

            // ============================
            // ATTENDANCE RULES
            // ============================

            int fullPresent = att.Count(a => a.Status == "P");

            int halfMorning = att.Count(a => a.Status == "½P");
            int halfEvening = att.Count(a => a.Status == "P½");
            int totalHalfDays = halfMorning + halfEvening;

            int weekOffPaid = att.Count(a => a.Status == "WO");
            int saturdayPaid = att.Count(a => a.Status == "WOP");

            int unpaidLeave = att.Count(a => a.Status == "L"); // PL/CL/SL = unpaid
            int absent = att.Count(a => a.Status == "A");

            // ============================
            // PAID DAYS CALCULATION
            // ============================
            decimal paidDays =
                (fullPresent * 1.0m) +
                (totalHalfDays * 0.5m) +
                (weekOffPaid * 1.0m) +
                (saturdayPaid * 1.0m) +
                (unpaidLeave * 0.0m);     // Leave unpaid

            decimal totalAbsent = absent + unpaidLeave;

            // ============================
            // PROFESSIONAL TAX
            // ============================
            string gender = emp?.Gender ?? "male"; // adjust field name if needed

            decimal pt = CalculatePT(payroll.net_salary ?? 0, gender, month);

            // ============================
            // BUILD VIEW MODEL
            // ============================
            return new PayrollSummaryVm
            {
                EmpCode = payroll.emp_code,
                EmpName = payroll.name,

                Year = year,
                Month = month,

                TotalDaysInMonth = payroll.working_days ?? DateTime.DaysInMonth(year, month),
                AbsentDays = totalAbsent,
                PresentHalfDays = totalHalfDays,
                WeeklyOffDays = weekOffPaid,
                TotalSaturdayPaid = saturdayPaid,
                LateMarks = payroll.late_marks ?? 0,
                LateDeductionDays = payroll.late_deduction_days ?? 0,
                PaidDays = paidDays,

                MonthlySalary = payroll.base_salary ?? 0,
                PerDaySalary = payroll.per_day_salary ?? 0,
                GrossSalary = payroll.gross_salary ?? 0,

                PerformanceAllowance = payroll.perf_allowance ?? 0,
                OtherAllowances = payroll.other_allowance ?? 0,
                PetrolAllowance = payroll.petrol_allowance ?? 0,
                Reimbursement = payroll.reimbursement ?? 0,

                ProfessionalTax = pt,
                TotalDeductions = (payroll.total_deduction ?? 0) + pt,
                NetSalary = (payroll.gross_salary ?? 0) - ((payroll.total_deduction ?? 0) + pt),
                TotalPay = payroll.total_pay ?? 0,

                BankName = emp?.BankName ?? "-",
                AccountNumber = emp?.AccountNumber ?? "-",
                IFSCCode = emp?.IFSC ?? "-",
                BankBranch = emp?.Branch ?? "-",

                Department = emp?.Department ?? "-",
                Designation = emp?.Position ?? "-"
            };
        }

        // ============================
        // BUILD SUMMARY FOR ALL EMPLOYEES
        // ============================
        public List<PayrollSummaryVm> GetMonthlySummaries(int year, int month)
        {
            string monthName = new DateTime(year, month, 1)
                .ToString("MMMM").ToUpper();

            var empCodes = _context.Payroll
                .Where(p => p.month.ToUpper() == monthName)
                .Select(p => p.emp_code)
                .Distinct()
                .ToList();

            var list = new List<PayrollSummaryVm>();

            foreach (var code in empCodes)
            {
                var vm = BuildMonthlySummary(code, year, month);
                if (vm != null)
                    list.Add(vm);
            }

            return list.OrderBy(x => x.EmpName).ToList();
        }
    }
}
