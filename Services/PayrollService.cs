using System;
using System.Collections.Generic;
using System.Linq;
using HRMS.Models;
using HRMS.Models.ViewModels;

namespace HRMS.Services
{
    public class PayrollService
    {
        /// <summary>
        /// Main payroll calculation service.
        /// Uses Employee + Attendance rows.
        /// Returns PayrollSummaryVm for Monthly.cshtml and Payslip.cshtml.
        /// </summary>
        public PayrollSummaryVm CalculatePayroll(
            Employee emp,
            List<Attendance> rows,
            int year,
            int month)
        {
            if (emp == null)
                throw new ArgumentNullException(nameof(emp));

            if (rows == null)
                rows = new List<Attendance>();

            int totalDays = DateTime.DaysInMonth(year, month);

            // ---------- ATTENDANCE COUNTS ----------
            int presentFull = rows.Count(r =>
                r.Status == "P" || r.Status == "WOP");

            int presentHalf = rows.Count(r =>
                r.Status == "½P" || r.Status == "P½" || r.Status == "HP");

            int weeklyOff = rows.Count(r => r.Status == "WO");
            int absent = rows.Count(r => r.Status == "A");

            int lateMarks = rows.Count(r => r.IsLate);
            int lateMarksOver3 = Math.Max(0, lateMarks - 3);

            // Late deduction: every 3 late marks after first 3 = 0.5 days
            decimal lateDeductionDays = (lateMarksOver3 / 3) * 0.5m;

            // Day presented (attendance-based)
            decimal dayPresented =
                presentFull +
                (presentHalf * 0.5m) +
                weeklyOff;

            // Paid days (after late deduction)
            decimal paidDays = dayPresented - lateDeductionDays;
            if (paidDays < 0) paidDays = 0;

            // ---------- SALARY ----------
            decimal baseSalary = emp.Salary ?? 0;
            decimal perDaySalary = totalDays == 0 ? 0 : (baseSalary / totalDays);

            // Optional allowances (set to 0 if Employee table doesn't contain them)
            decimal performanceAllowance = 0;
            decimal otherAllowances = 0;
            decimal petrolAllowance = 0;
            decimal reimbursement = 0;

            // Professional Tax rule
            decimal professionalTax = baseSalary < 10000 ? 0 : 200;

            // Gross salary
            decimal grossSalary =
                (perDaySalary * paidDays) +
                performanceAllowance +
                otherAllowances +
                petrolAllowance +
                reimbursement;

            // Total deductions & net salary
            decimal totalDeductions = professionalTax;
            decimal netSalary = grossSalary - totalDeductions;
            if (netSalary < 0) netSalary = 0;

            // ---------- RETURN FINAL VIEW MODEL ----------
            return new PayrollSummaryVm
            {
                EmpCode = emp.EmployeeCode,
                EmpName = emp.Name,

                Year = year,
                Month = month,

                TotalDaysInMonth = totalDays,
                PresentHalfDays = presentHalf,
                WeeklyOffDays = weeklyOff,
                AbsentDays = absent,

                LateMarks = lateMarks,
                LateDeductionDays = lateDeductionDays,
                PaidDays = paidDays,

                MonthlySalary = baseSalary,
                PerDaySalary = perDaySalary,
                GrossSalary = grossSalary,

                PerformanceAllowance = performanceAllowance,
                OtherAllowances = otherAllowances,
                PetrolAllowance = petrolAllowance,
                Reimbursement = reimbursement,

                ProfessionalTax = professionalTax,
                TotalDeductions = totalDeductions,
                NetSalary = netSalary
            };
        }
    }
}
