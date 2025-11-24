using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using HRMS.Data;
using HRMS.Models;
using HRMS.Models.ViewModels;
using HRMS.Services;

namespace HRMS.Controllers
{
    public class PayrollController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PayrollController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= MONTHLY PAYROLL SCREEN =================
        public IActionResult Monthly(int? year, int? month)
        {
            int y = year ?? DateTime.Today.Year;
            int m = month ?? DateTime.Today.Month;

            ViewBag.Year = y;
            ViewBag.Month = m;

            string monthName = new DateTime(y, m, 1).ToString("MMMM").ToUpper();

            // 1️⃣ CHECK IF PAYROLL IS SAVED IN DB (MonthYear = "MAY")
            var saved = _context.Payroll
                .Where(p => p.Month.ToUpper() == monthName)
                .ToList();

            if (saved.Any())
            {
                var vmList = saved.Select(p => new PayrollSummaryVm
                {
                    EmpCode = p.EmployeeCode,
                    EmpName = p.Name,
                    Year = y,
                    Month = m,

                    TotalDaysInMonth = p.WorkingDays,

                    PresentHalfDays = 0,
                    WeeklyOffDays = 0,
                    AbsentDays = p.LeavesTaken,

                    LateMarks = p.LateMarks,
                    LateDeductionDays = p.LateDeductionDays,
                    PaidDays = p.PaidDays,

                    MonthlySalary = p.BaseSalary,
                    PerDaySalary = p.BaseSalary / (p.WorkingDays == 0 ? 1 : p.WorkingDays),
                    GrossSalary = p.GrossSalary,

                    PerformanceAllowance = p.PerfAllowance,
                    OtherAllowances = p.OtherAllowance,
                    PetrolAllowance = p.PetrolAllowance,
                    Reimbursement = p.Reimbursement,

                    ProfessionalTax = p.ProfTax,
                    TotalDeductions = p.TotalDeduction,
                    NetSalary = p.NetSalary
                }).ToList();

                return View(vmList);
            }

            // 2️⃣ FALLBACK — GENERATE USING ATTENDANCE
            var employees = _context.Employees.ToList();
            var service = new PayrollService();
            var result = new List<PayrollSummaryVm>();

            foreach (var emp in employees)
            {
                var rows = _context.Attendances
                    .Where(a =>
                        a.Emp_Code == emp.EmployeeCode &&
                        a.Date.Year == y &&
                        a.Date.Month == m)
                    .ToList();

                if (rows.Any())
                {
                    var summary = service.CalculatePayroll(emp, rows, y, m);
                    result.Add(summary);
                }
            }

            return View(result);
        }

        // ================= PAYSLIP =================
        public IActionResult Payslip(string empCode, int year, int month)
        {
            var emp = _context.Employees
                .FirstOrDefault(e => e.EmployeeCode == empCode);

            if (emp == null)
                return NotFound("Employee not found.");

            var rows = _context.Attendances
                .Where(a =>
                    a.Emp_Code == empCode &&
                    a.Date.Year == year &&
                    a.Date.Month == month)
                .ToList();

            if (!rows.Any())
                return NotFound("No attendance data available.");

            var service = new PayrollService();
            var summary = service.CalculatePayroll(emp, rows, year, month);

            return View(summary);
        }
    }
}
