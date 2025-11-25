using HRMS.Data;
using HRMS.Models;
using HRMS.Models.ViewModels;
using HRMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HRMS.Controllers
{
  
    public class PayrollController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public PayrollController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
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
            var emp = _context.Employees.FirstOrDefault(e => e.EmployeeCode == empCode);

            if (emp == null)
                return NotFound("Employee not found.");

            var rows = _context.Attendances
                .Where(a => a.Emp_Code == empCode && a.Date.Year == year && a.Date.Month == month)
                .ToList();

            // ✅ If no attendance data, try pulling from Payroll table
            if (!rows.Any())
            {
                var saved = _context.Payroll
                    .FirstOrDefault(p => p.EmployeeCode == empCode &&
                     p.Month.ToUpper() == new DateTime(year, month, 1).ToString("MMMM").ToUpper());


                if (saved == null)
                    return NotFound("No attendance or payroll data available.");

                var vm = new PayrollSummaryVm
                {
                    EmpCode = emp?.EmployeeCode ?? string.Empty,
                    EmpName = emp?.Name ?? string.Empty,
                    Department = emp?.Department ?? string.Empty,
                    Designation = emp?.Position ?? string.Empty,
                    BankName = emp?.BankName ?? string.Empty,
                    AccountNumber = emp?.AccountNumber ?? string.Empty,
                    IFSCCode = emp?.IFSC ?? string.Empty,
                    BankBranch = emp?.Branch ?? string.Empty,

                    Year = year,
                    Month = month,
                    TotalDaysInMonth = saved.WorkingDays,
                    AbsentDays = saved.LeavesTaken,
                    LateMarks = saved.LateMarks,
                    LateDeductionDays = saved.LateDeductionDays,
                    PaidDays = saved.PaidDays,
                    MonthlySalary = saved.BaseSalary,
                    PerDaySalary = saved.PerDaySalary ?? 0,
                    GrossSalary = saved.GrossSalary ?? 0,
                    PerformanceAllowance = saved.PerfAllowance ?? 0,
                    OtherAllowances = saved.OtherAllowance ?? 0,
                    PetrolAllowance = saved.PetrolAllowance ?? 0,
                    Reimbursement = saved.Reimbursement ?? 0,
                    ProfessionalTax = saved.ProfTax ?? 0,
                    TotalDeductions = saved.TotalDeduction ?? 0,
                    NetSalary = saved.NetSalary ?? 0
                };


                return View(vm);
            }

            // ✅ If attendance exists, calculate payroll normally
            var service = new PayrollService();
            var summary = service.CalculatePayroll(emp, rows, year, month);

            // Add Employee Info
            summary.Department = emp.Department;
            summary.Designation = emp.Position;

            // ✅ Add Bank Info
            summary.BankName = emp.BankName;
            summary.AccountNumber = emp.AccountNumber;
            summary.IFSCCode = emp.IFSC;
            summary.BankBranch = emp.Branch;

            return View(summary);
        }

        [HttpGet]
        public IActionResult DownloadSalarySlip(int month, int year)
        {
            // Get the logged-in user’s employee record
            var employee = _context.Employees.FirstOrDefault(e => e.Email == User.Identity.Name);
            if (employee == null)
                return NotFound("Employee not found.");

            // Example file name format: EMP001_4_2025.pdf
            var fileName = $"{employee.EmployeeCode}_{month}_{year}.pdf";
            var filePath = Path.Combine(_env.WebRootPath, "SalarySlips", fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound($"Salary slip for {month}/{year} not found.");
            }

            // Return the PDF file to the browser
            return PhysicalFile(filePath, "application/pdf", fileName);
        }

    }
}
