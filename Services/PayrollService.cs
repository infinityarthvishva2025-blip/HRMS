using System;
using System.Collections.Generic;
using System.Linq;
using HRMS.Models;
using HRMS.Models.ViewModels;

namespace HRMS.Services
{
    public class PayrollService
    {
        public PayrollSummaryVm CalculatePayroll(Employee emp, List<Attendance> rows, int year, int month)
        {
            if (emp == null) throw new ArgumentNullException(nameof(emp));
            if (rows == null) rows = new List<Attendance>();

            int totalDays = DateTime.DaysInMonth(year, month);

            // ================= ATTENDANCE =================
            int presentFull = rows.Count(r => r.Status == "P" || r.Status == "WOP");
            int presentHalf = rows.Count(r => r.Status == "½P" || r.Status == "P½" || r.Status == "HP");
            int weeklyOff = rows.Count(r => r.Status == "WO");
            int absent = rows.Count(r => r.Status == "A");

            int lateMarks = rows.Count(r => r.IsLate);
            int lateMarksOver3 = Math.Max(0, lateMarks - 3);
            decimal lateDedDays = (lateMarksOver3 / 3) * 0.5m;

            decimal dayPresented =
                  presentFull
                + (presentHalf * 0.5m)
                + weeklyOff;

            decimal paidDays = dayPresented - lateDedDays;
            if (paidDays < 0) paidDays = 0;

            // ================= SALARY =================
            decimal baseSalary = emp.Salary ?? 0m;
            decimal perDaySalary = baseSalary / totalDays;

            decimal performanceAllowance = 0;
            decimal otherAllowances = 0;
            decimal petrolAllowance = 0;
            decimal reimbursement = 0;

            decimal professionalTax = baseSalary < 10000 ? 0 : 200;

            decimal grossSalary =
                (perDaySalary * paidDays) +
                performanceAllowance +
                otherAllowances +
                petrolAllowance +
                reimbursement;

            decimal totalDeductions = professionalTax;
            decimal netSalary = grossSalary - totalDeductions;
            if (netSalary < 0) netSalary = 0;

            return new PayrollSummaryVm
            {
                EmpCode = emp.EmployeeCode,
                EmpName = emp.Name,
                Year = year,
                Month = month,

                TotalDaysInMonth = totalDays,
                PresentFullDays = presentFull,
                PresentHalfDays = presentHalf,
                WeeklyOffDays = weeklyOff,
                AbsentDays = absent,

                LateMarks = lateMarks,
                LateMarksOver3 = lateMarksOver3,
                LateDeductionDays = lateDedDays,

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
                NetSalary = netSalary,
            };
        }
    }
}
