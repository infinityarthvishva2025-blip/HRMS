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
        // PROFESSIONAL TAX – MAHARASHTRA (FINAL CORRECT LOGIC)
        // ============================================================
        public decimal CalculatePT(decimal basicSalary, string gender, int month)
        {
            gender = (gender ?? "male").Trim().ToLower();

            // Female employees
            if (gender == "female")
            {
                if (basicSalary <= 25000m)
                    return 0m;

                return (month == 1) ? 300m : 200m;
            }

            // Male employees
            if (basicSalary <= 7500m)
                return 0m;

            if (basicSalary <= 10000m)
                return 175m;

            return (month == 1) ? 300m : 200m;
        }

        // ============================================================
        // PAYROLL BY DATE RANGE
        // ============================================================
        public PayrollSummaryVm BuildPayrollByDateRange(
    string empCode,
    DateTime fromDate,
    DateTime toDate)
        {
            var emp = _context.Employees
                .FirstOrDefault(e =>
                    e.EmployeeCode == empCode &&
                    e.Status == "Active" &&
                    (e.LastWorkingDate == null || e.LastWorkingDate >= fromDate) &&
                    e.JoiningDate <= toDate);

            if (emp == null)
                return null;

            decimal baseSalary = emp.Salary ?? 0m;

            // 🔥 FIX: Adjust end date based on Last Working Date
            DateTime effectiveToDate = toDate;

            if (emp.LastWorkingDate.HasValue &&
                emp.LastWorkingDate.Value < toDate)
            {
                effectiveToDate = emp.LastWorkingDate.Value;
            }

            var attendance = _context.Attendances
                .Where(a =>
                    a.Emp_Code == empCode &&
                    a.Date.Date >= fromDate.Date &&
                    a.Date.Date <= effectiveToDate.Date)
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

                if (day == DayOfWeek.Sunday || status == "WO")
                {
                    weeklyOffDays++;
                    fullDays++;
                    continue;
                }

                if (status == "H" || status == "HO" || status == "COFF")
                {
                    fullDays++;
                    continue;
                }

                if (status == "A" || status == "L")
                {
                    absentDays++;
                    continue;
                }

                if (!a.InTime.HasValue || !a.OutTime.HasValue)
                {
                    halfDays++;
                    continue;
                }

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
            }

            if ((fullDays - weeklyOffDays) == 0 && halfDays == 0)
                return null;

            decimal paidDays = fullDays + (halfDays * 0.5m);

            // 🔥 FIX: Total days should be till last working date
            int totalDaysInRange =
                (effectiveToDate.Date - fromDate.Date).Days + 1;

            int daysInMonth =
                DateTime.DaysInMonth(fromDate.Year, fromDate.Month);

            decimal perDaySalary = baseSalary / daysInMonth;

            decimal grossSalary = paidDays * perDaySalary;

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

                FromDate = fromDate,
                ToDate = effectiveToDate, // 🔥 updated

                TotalDaysInMonth = totalDaysInRange,

                BankName = emp.BankName,
                AccountNumber = emp.AccountNumber,
                IFSCCode = emp.IFSC,

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
        
        public PayrollSummaryVm BuildMonthlySummary(string empCode, int year, int month)
        {
            DateTime start = new DateTime(year, month, 1);
            DateTime end = start.AddMonths(1).AddDays(-1);

            return BuildPayrollByDateRange(empCode, start, end);
        }

        public List<PayrollSummaryVm> GetMonthlySummaries(int year, int month)
        {
            DateTime start = new DateTime(year, month, 1);
            DateTime end = start.AddMonths(1).AddDays(-1);

            var empCodes = _context.Employees
                .Where(e =>
                    e.Status == "Active" &&
                    (e.LastWorkingDate == null || e.LastWorkingDate >= start) &&
                    e.JoiningDate <= end)
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
//using HRMS.Data;
//using HRMS.Models;
//using HRMS.Models.ViewModels;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace HRMS.Services
//{
//    public class PayrollService
//    {
//        private readonly ApplicationDbContext _context;

//        public PayrollService(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        // ============================================================
//        // PROFESSIONAL TAX – MAHARASHTRA
//        // ============================================================
//        //public decimal CalculatePT(decimal basicSalary, string gender, int month)
//        //{
//        //    gender = (gender ?? "male").Trim().ToLower();

//        //    if (month == 1)
//        //        return 300m;

//        //    if (gender == "female") 
//        //        return basicSalary < 25000m ? 0m : 200m;

//        //    if (basicSalary <= 7500m)
//        //        return 0m;

//        //    if (basicSalary <= 10000m)
//        //        return 175m;

//        //    return 0m;
//        //}
//        public decimal CalculatePT(decimal basicSalary, string gender, int month)
//        {
//            gender = (gender ?? "male").Trim().ToLower();

//            // Female employees
//            if (gender == "female")
//            {
//                if (basicSalary <= 25000m)
//                    return 0m;

//                // Above 25,000
//                return (month == 1) ? 300m : 200m;
//            }

//            // Male employees
//            if (basicSalary <= 7500m)
//                return 0m;

//            if (basicSalary <= 10000m)
//                return 175m; // No February change for this slab

//            // Above 10,000
//            return (month == 1) ? 300m : 200m;
//        }
//        // ============================================================
//        // PAYROLL BY DATE RANGE (FINAL VERSION WITH FINANCIAL YEAR COFF)
//        // ============================================================
//    //    public PayrollSummaryVm BuildPayrollByDateRange(
//    //string empCode,
//    //DateTime fromDate,
//    //DateTime toDate)
//    //    {
//    //        var emp = _context.Employees
//    //            .FirstOrDefault(e =>
//    //                e.EmployeeCode == empCode &&
//    //                e.Status == "Active");

//    //        if (emp == null)
//    //            return null;

//    //        decimal baseSalary = emp.Salary ?? 0m;

//    //        var attendance = _context.Attendances
//    //            .Where(a =>
//    //                a.Emp_Code == empCode &&
//    //                a.Date >= fromDate &&
//    //                a.Date <= toDate)
//    //            .ToList();

//    //        if (!attendance.Any())
//    //            return null;

//    //        int fullDays = 0;
//    //        int halfDays = 0;
//    //        int absentDays = 0;
//    //        int weeklyOffDays = 0;

//    //        foreach (var a in attendance)
//    //        {
//    //            string status = (a.Status ?? "").Trim().ToUpper();
//    //            var day = a.Date.DayOfWeek;

//    //            // =============================
//    //            // WEEKLY OFF
//    //            // =============================
//    //            if (day == DayOfWeek.Sunday || status == "WO")
//    //            {
//    //                weeklyOffDays++;
//    //                fullDays++;
//    //                continue;
//    //            }

//    //            // =============================
//    //            // HOLIDAY
//    //            // =============================
//    //            if (status == "H" || status == "HO")
//    //            {
//    //                fullDays++;
//    //                continue;
//    //            }

//    //            // =============================
//    //            // ABSENT / LEAVE
//    //            // =============================
//    //            if (status == "A" || status == "L")
//    //            {
//    //                absentDays++;
//    //                continue;
//    //            }

//    //            double workedHours = 0;

//    //            if (a.InTime.HasValue && a.OutTime.HasValue)
//    //            {
//    //                workedHours = (a.OutTime.Value - a.InTime.Value).TotalHours;
//    //            }

//    //            double fullDayHours =
//    //                day == DayOfWeek.Saturday ? 7.0 : 8.5;

//    //            double halfDayHours =
//    //                day == DayOfWeek.Saturday ? 3.5 : 4.0;

//    //            // =============================
//    //            // NO CHECKOUT (HALF DAY)
//    //            // =============================
//    //            if (a.InTime.HasValue && !a.OutTime.HasValue)
//    //            {
//    //                halfDays++;
//    //                continue;
//    //            }

//    //            // =============================
//    //            // ACTUAL HALF DAY
//    //            // =============================
//    //            if (workedHours >= halfDayHours && workedHours < fullDayHours)
//    //            {
//    //                halfDays++;
//    //                continue;
//    //            }

//    //            // =============================
//    //            // FULL DAY
//    //            // =============================
//    //            if (workedHours >= fullDayHours)
//    //            {
//    //                fullDays++;
//    //                continue;
//    //            }
//    //        }

//    //        // ============================================
//    //        // COFF BALANCE FROM LEDGER
//    //        // ============================================

//    //        var coffLedger = _context.CompOffLedgers
//    //            .Where(c => c.EmployeeId == emp.Id)
//    //            .ToList();

//    //        int coffEarned = coffLedger
//    //            .Count(c => c.Status == "Active");

//    //        int coffUsed = coffLedger
//    //            .Count(c => c.Status == "Used");

//    //        int coffPending = coffEarned - coffUsed;

//    //        if (coffPending < 0)
//    //            coffPending = 0;

//    //        decimal coffPaymentDays = 0;

//    //        // ============================================
//    //        // FINANCIAL YEAR END (MARCH)
//    //        // ============================================

//    //        if (toDate.Month == 3)
//    //        {
//    //            coffPaymentDays = coffPending;
//    //            coffPending = 0;
//    //        }

//    //        // ============================================
//    //        // SALARY CALCULATION
//    //        // ============================================

//    //        decimal paidDays =
//    //            fullDays +
//    //            (halfDays * 0.5m) +
//    //            coffPaymentDays;

//    //        int daysInMonth =
//    //            DateTime.DaysInMonth(fromDate.Year, fromDate.Month);

//    //        decimal perDaySalary =
//    //            baseSalary / daysInMonth;

//    //        decimal grossSalary =
//    //            paidDays * perDaySalary;

//    //        decimal professionalTax =
//    //            CalculatePT(baseSalary, emp.Gender, fromDate.Month);

//    //        decimal netSalary =
//    //            grossSalary - professionalTax;

//    //        int totalDaysInRange =
//    //            (toDate.Date - fromDate.Date).Days + 1;

//    //        return new PayrollSummaryVm
//    //        {
//    //            EmpCode = empCode,
//    //            EmpName = emp.Name,
//    //            Department = emp.Department,
//    //            Designation = emp.Position,

//    //            Year = fromDate.Year,
//    //            Month = fromDate.Month,

//    //            FromDate = fromDate,
//    //            ToDate = toDate,

//    //            BankName = emp.BankName,
//    //            AccountNumber = emp.AccountNumber,
//    //            IFSCCode = emp.IFSC,

//    //            PresentHalfDays = halfDays,
//    //            WeeklyOffDays = weeklyOffDays,
//    //            AbsentDays = absentDays,

//    //            PaidDays = paidDays,

//    //            MonthlySalary = baseSalary,
//    //            PerDaySalary = perDaySalary,
//    //            GrossSalary = grossSalary,

//    //            TotalDaysInMonth = totalDaysInRange,

//    //            ProfessionalTax = professionalTax,
//    //            TotalDeductions = professionalTax,

//    //            CoffPending = coffPending,
//    //            CoffPaymentDays = coffPaymentDays,

//    //            NetSalary = netSalary,
//    //            TotalPay = netSalary
//    //        };
//    //    }
//        public PayrollSummaryVm BuildPayrollByDateRange(
//            string empCode,
//            DateTime fromDate,
//            DateTime toDate)
//        {
//            var emp = _context.Employees
//                .FirstOrDefault(e =>
//                    e.EmployeeCode == empCode &&
//                    e.Status == "Active");

//            if (emp == null)
//                return null;

//            decimal baseSalary = emp.Salary ?? 0m;

//            var attendance = _context.Attendances
//                .Where(a =>
//                    a.Emp_Code == empCode &&
//                    a.Date.Date >= fromDate.Date &&
//                    a.Date.Date <= toDate.Date)
//                .ToList();

//            if (!attendance.Any())
//                return null;

//            int fullDays = 0;
//            int halfDays = 0;
//            int absentDays = 0;
//            int weeklyOffDays = 0;

//            foreach (var a in attendance)
//            {
//                string status = (a.Status ?? "").Trim().ToUpper();

//                var day = a.Date.DayOfWeek;

//                // =====================================================
//                // WEEKLY OFF
//                // =====================================================
//                if (day == DayOfWeek.Sunday || status == "WO")
//                {
//                    weeklyOffDays++;
//                    fullDays++;
//                    continue;
//                }

//                // =====================================================
//                // HOLIDAY / COFF
//                // =====================================================
//                if (status == "H" || status == "HO" || status == "COFF")
//                {
//                    fullDays++;
//                    continue;
//                }

//                // =====================================================
//                // ABSENT / LEAVE
//                // =====================================================
//                if (status == "A" || status == "L")
//                {
//                    absentDays++;
//                    continue;
//                }

//                // =====================================================
//                // MISSING PUNCH
//                // =====================================================
//                if (!a.InTime.HasValue || !a.OutTime.HasValue)
//                {
//                    halfDays++;
//                    continue;
//                }

//                // =====================================================
//                // WORKING HOURS CALCULATION
//                // =====================================================
//                double workedHours =
//                    (a.OutTime.Value - a.InTime.Value).TotalHours;

//                double fullDayHours =
//                    day == DayOfWeek.Saturday ? 7.0 : 8.5;

//                double halfDayHours =
//                    day == DayOfWeek.Saturday ? 3.5 : 4.0;

//                if (workedHours >= fullDayHours)
//                    fullDays++;
//                else if (workedHours >= halfDayHours)
//                    halfDays++;
//                //else
//                //    halfDays++; //absentDays++;
//            }

//            // =====================================================
//            // CHECK IF EMPLOYEE NEVER WORKED
//            // (Only Absent + WeeklyOff)
//            // =====================================================
//            if ((fullDays - weeklyOffDays) == 0 && halfDays == 0)
//            {
//                return null;
//            }

//            // =====================================================
//            // SALARY CALCULATION
//            // =====================================================
//            decimal paidDays = fullDays + (halfDays * 0.5m);

//            int totalDaysInRange =
//                (toDate.Date - fromDate.Date).Days + 1;

//            int daysInMonth =
//                DateTime.DaysInMonth(fromDate.Year, fromDate.Month);

//            decimal perDaySalary = baseSalary / daysInMonth;

//            decimal grossSalary = paidDays * perDaySalary;

//            decimal professionalTax =
//                CalculatePT(baseSalary, emp.Gender, fromDate.Month);

//            decimal netSalary = grossSalary - professionalTax;

//            return new PayrollSummaryVm
//            {
//                EmpCode = empCode,
//                EmpName = emp.Name,
//                Department = emp.Department,
//                Designation = emp.Position,

//                Year = fromDate.Year,
//                Month = fromDate.Month,

//                FromDate = fromDate,
//                ToDate = toDate,

//                TotalDaysInMonth = totalDaysInRange,
//                BankName = emp.BankName,
//                AccountNumber = emp.AccountNumber,
//                IFSCCode = emp.IFSC,
//                PresentHalfDays = halfDays,
//                WeeklyOffDays = weeklyOffDays,
//                AbsentDays = absentDays,

//                PaidDays = paidDays,

//                MonthlySalary = baseSalary,
//                PerDaySalary = perDaySalary,
//                GrossSalary = grossSalary,

//                ProfessionalTax = professionalTax,
//                TotalDeductions = professionalTax,

//                NetSalary = netSalary,
//                TotalPay = netSalary
//            };
//        }

//         //============================================================
//         //MONTHLY PAYROLL
//         //============================================================
//        public PayrollSummaryVm BuildMonthlySummary(string empCode, int year, int month)
//        {
//            DateTime start = new DateTime(year, month, 1);
//            DateTime end = start.AddMonths(1).AddDays(-1);

//            return BuildPayrollByDateRange(empCode, start, end);
//        }

//        public List<PayrollSummaryVm> GetMonthlySummaries(int year, int month)
//        {
//            DateTime start = new DateTime(year, month, 1);
//            DateTime end = start.AddMonths(1).AddDays(-1);

//            var empCodes = _context.Employees
//                .Select(e => e.EmployeeCode)
//                .ToList();

//            List<PayrollSummaryVm> result = new();

//            foreach (var code in empCodes)
//            {
//                var summary = BuildPayrollByDateRange(code, start, end);
//                if (summary != null)
//                    result.Add(summary);
//            }

//            return result.OrderBy(x => x.EmpName).ToList();
//        }
//    }
//}


