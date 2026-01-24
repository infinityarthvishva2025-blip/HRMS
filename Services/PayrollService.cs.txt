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
        // PROFESSIONAL TAX – MAHARASHTRA (FINAL & CORRECT)
        // ============================================================
        public decimal CalculatePT(decimal basicSalary, string gender, int month)
        {
            gender = (gender ?? "male").Trim().ToLower();

            //  February special Professional Tax (once per year)
            if (month == 2)
                return 300m;

            //  Female employees
            if (gender == "female")
            {
                return basicSalary <= 25000m ? 0m : 200m;
            }

            // Male employees
            if (basicSalary <= 7500m)
                return 0m;

            if (basicSalary <= 10000m)
                return 175m;

            return 200m;
        }

        // ============================================================
        // PAYROLL BY DATE RANGE (WORKING HOURS DRIVEN)
        // ============================================================
        public PayrollSummaryVm BuildPayrollByDateRange(
     string empCode,
     DateTime fromDate,
     DateTime toDate)
        {
            var emp = _context.Employees
                .FirstOrDefault(e => e.EmployeeCode == empCode);

            if (emp == null)
                return null;

            decimal baseSalary = emp.Salary ?? 0m;

            var attendance = _context.Attendances
                .Where(a =>
                    a.Emp_Code == empCode &&
                    a.Date >= fromDate.Date &&
                    a.Date <= toDate.Date)
                .ToList();

            if (!attendance.Any())
                return null;

            int fullDays = 0;
            int halfDays = 0;
            int absentDays = 0;
            int weeklyOffDays = 0;

            foreach (var a in attendance)
            {
                string status = (a.Status ?? "").Trim().ToUpper();
                var day = a.Date.DayOfWeek;

                // ✅ Sunday = Weekly Off (Paid)
                if (day == DayOfWeek.Sunday)
                {
                    weeklyOffDays++;
                    fullDays++;
                    continue;
                }

                // ✅ Holiday = Paid
                if (status == "H" || status == "HO")
                {
                    fullDays++;
                    continue;
                }

                // ❌ Leave / Absent
                if (status == "A" || status == "L")
                {
                    absentDays++;
                    continue;
                }

                // ❌ Missing punch = Absent
                if (!a.InTime.HasValue || !a.OutTime.HasValue)
                {
                    absentDays++;
                    continue;
                }

                // ⏱️ Calculate working hours from punch time
                double workedHours =
                    (a.OutTime.Value - a.InTime.Value).TotalHours;

                double fullDayHours =
                    day == DayOfWeek.Saturday ? 7.0 : 8.5;

                double halfDayHours =
                    day == DayOfWeek.Saturday ? 3.5 : 4.0;

                if (workedHours >= fullDayHours)
                    fullDays++;
                else if (workedHours >= halfDayHours)
                    halfDays++;
                else
                    absentDays++;
            }

            // ✅ Paid days calculation
            decimal paidDays = fullDays + (halfDays * 0.5m);

            // ✅ Date-range working days
            int totalDaysInRange =
                (toDate.Date - fromDate.Date).Days + 1;

            // ✅ Per-day salary ALWAYS based on full month
            int daysInMonth =
                DateTime.DaysInMonth(fromDate.Year, fromDate.Month);

            decimal perDaySalary = baseSalary / daysInMonth;

            // ✅ Gross salary ONLY for selected range
            decimal grossSalary = paidDays * perDaySalary;

            // ✅ Professional Tax (month-based rule)
            decimal professionalTax =
                CalculatePT(baseSalary, emp.Gender, fromDate.Month);

            decimal netSalary = grossSalary - professionalTax;

            return new PayrollSummaryVm
            {
                EmpCode = empCode,
                EmpName = emp.Name,
                Department = emp.Department,
                Designation = emp.Position,

                Year = fromDate.Year,
                Month = fromDate.Month,

                // 🔥 IMPORTANT FOR PDF / VIEW
                FromDate = fromDate,
                ToDate = toDate,

                TotalDaysInMonth = totalDaysInRange,

                PresentHalfDays = halfDays,
                WeeklyOffDays = weeklyOffDays,
                AbsentDays = absentDays,

                PaidDays = paidDays,

                MonthlySalary = baseSalary,
                PerDaySalary = perDaySalary,
                GrossSalary = grossSalary,

                ProfessionalTax = professionalTax,
                TotalDeductions = professionalTax,

                NetSalary = netSalary,
                TotalPay = netSalary
            };
        }


        // ============================================================
        // MONTHLY PAYROLL (FULL MONTH)
        // ============================================================
        public PayrollSummaryVm BuildMonthlySummary(string empCode, int year, int month)
        {
            DateTime start = new DateTime(year, month, 1);
            DateTime end = start.AddMonths(1).AddDays(-1);

            return BuildPayrollByDateRange(empCode, start, end);
        }

        // ============================================================
        // MONTHLY PAYROLL – ALL EMPLOYEES
        // ============================================================
        public List<PayrollSummaryVm> GetMonthlySummaries(int year, int month)
        {
            DateTime start = new DateTime(year, month, 1);
            DateTime end = start.AddMonths(1).AddDays(-1);

            var empCodes = _context.Employees
                .Select(e => e.EmployeeCode)
                .ToList();

            List<PayrollSummaryVm> result = new();

            foreach (var code in empCodes)
            {
                var summary = BuildPayrollByDateRange(code, start, end);
                if (summary != null)
                    result.Add(summary);
            }

            return result.OrderBy(x => x.EmpName).ToList();
        }
    }
}
