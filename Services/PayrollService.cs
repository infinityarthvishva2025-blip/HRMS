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
        // PROFESSIONAL TAX (Maharashtra)
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
            else // Male
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
            string monthName = new DateTime(year, month, 1).ToString("MMMM").ToUpper();

            var payroll = _context.Payroll
                .FirstOrDefault(p =>
                    p.emp_code == empCode &&
                    p.month.ToUpper() == monthName);

            if (payroll == null)
                return null;

            var emp = _context.Employees.FirstOrDefault(e => e.EmployeeCode == empCode);

            // ============================
            // FETCH ATTENDANCE DATA
            // ============================
            var start = new DateTime(year, month, 1);
            var end = start.AddMonths(1).AddDays(-1);

            var att = _context.Attendances
                .Where(a => a.Emp_Code == empCode && a.Date >= start && a.Date <= end)
                .ToList();

            string Clean(string s) => (s ?? "").Trim().ToUpper();

            // ============================
            // ATTENDANCE COUNTS
            // ============================
            int fullPresent = att.Count(a => Clean(a.Status) == "P");
            int halfMorning = att.Count(a => Clean(a.Status) == "½P");
            int halfEvening = att.Count(a => Clean(a.Status) == "P½");
            int totalHalfDays = halfMorning + halfEvening;

            // ✅ Weekly Off (WO / variants) — PAID
            int weekOffPaid = att.Count(a =>
                Clean(a.Status) == "WO" ||
                Clean(a.Status) == "W/O" ||
                Clean(a.Status) == "WEEKOFF" ||
                Clean(a.Status) == "WO½P");

            // ✅ Saturday (WOP / variants) — PAID
            int saturdayPaid = att.Count(a =>
                Clean(a.Status) == "WOP" ||
                Clean(a.Status) == "SATWOP" ||
                Clean(a.Status) == "SATURDAY" ||
                Clean(a.Status) == "SAT");

            // Leaves & Absence
            int unpaidLeave = att.Count(a => Clean(a.Status) == "L");
            int paidLeave = att.Count(a =>
                Clean(a.Status) == "PL" ||
                Clean(a.Status) == "CL" ||
                Clean(a.Status) == "SL");
            int absent = att.Count(a => Clean(a.Status) == "A");

            // ============================
            // LATE MARK DEDUCTION RULE
            // ============================
            int totalLateMarks = payroll.late_marks ?? 0;
            decimal lateDeductionDays = 0m;

            if (totalLateMarks > 3)
            {
                int extraLates = totalLateMarks - 3;
                lateDeductionDays = extraLates * 0.5m; // Each extra late = 0.5 day
            }

            // ============================
            // PAID DAYS CALCULATION
            // ============================
            decimal paidDays =
                (fullPresent * 1.0m) +
                (totalHalfDays * 0.5m) +
                (paidLeave * 1.0m) +
                (weekOffPaid * 1.0m) +
                (saturdayPaid * 1.0m) -
                lateDeductionDays;

            // ============================
            // ABSENT / LOSS OF PAY DAYS
            // ============================
            // Only A + unpaid leaves (WO/WOP excluded)
            decimal totalAbsent = absent + unpaidLeave + lateDeductionDays;

            int totalDays = DateTime.DaysInMonth(year, month);

            // ============================
            // SALARY CALCULATIONS
            // ============================
            decimal monthlyBase = payroll.base_salary ?? 0;
            decimal perDaySalary = monthlyBase / totalDays;
            decimal grossSalary = perDaySalary * paidDays;

            // Professional Tax
            string gender = emp?.Gender ?? "male";
            decimal pt = CalculatePT(grossSalary, gender, month);

            // Deductions
            decimal otherDeductions = payroll.total_deduction ?? 0;
            decimal totalDeductions = otherDeductions + pt;
            decimal netSalary = grossSalary - totalDeductions;

            // ============================
            // 🔄 AUTO-SYNC Payroll TABLE
            // ============================
            payroll.leaves_taken = (decimal)totalAbsent;
            payroll.day_presented = (decimal)paidDays;
            _context.SaveChanges();

            // ============================
            // BUILD VIEW MODEL
            // ============================
            return new PayrollSummaryVm
            {
                EmpCode = payroll.emp_code,
                EmpName = payroll.name,
                Year = year,
                Month = month,

                // Attendance
                TotalDaysInMonth = totalDays,
                AbsentDays = totalAbsent,
                PresentHalfDays = totalHalfDays,
                WeeklyOffDays = weekOffPaid,
                TotalSaturdayPaid = saturdayPaid,
                PaidDays = paidDays,
                LateMarks = totalLateMarks,
                LateDeductionDays = lateDeductionDays,

                // Salary
                MonthlySalary = monthlyBase,
                PerDaySalary = perDaySalary,
                GrossSalary = grossSalary,
                PerformanceAllowance = payroll.perf_allowance ?? 0,
                OtherAllowances = payroll.other_allowance ?? 0,
                PetrolAllowance = payroll.petrol_allowance ?? 0,
                Reimbursement = payroll.reimbursement ?? 0,

                // Deductions
                ProfessionalTax = pt,
                OtherDeductions = otherDeductions,
                TotalDeductions = totalDeductions,
                NetSalary = netSalary,
                TotalPay = netSalary,

                // Employee Info
                Department = emp?.Department ?? "-",
                Designation = emp?.Position ?? "-",

                // Bank Info
                BankName = emp?.BankName ?? "-",
                AccountNumber = emp?.AccountNumber ?? "-",
                IFSCCode = emp?.IFSC ?? "-",
                BankBranch = emp?.Branch ?? "-"
            };
        }

        // ============================
        // BUILD SUMMARY FOR ALL EMPLOYEES
        // ============================
        public List<PayrollSummaryVm> GetMonthlySummaries(int year, int month)
        {
            string monthName = new DateTime(year, month, 1).ToString("MMMM").ToUpper();

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
