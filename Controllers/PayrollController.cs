using System;
using System.Linq;
using System.Collections.Generic;
using HRMS.Data;
using HRMS.Models;
using HRMS.Models.ViewModels;
using HRMS.Services;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Controllers
{
    public class PayrollController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PayrollController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= MONTHLY PAYROLL TABLE =================
        public IActionResult Monthly(int? year, int? month)
        {
            int y = year ?? DateTime.Today.Year;
            int m = month ?? DateTime.Today.Month;

            var employees = _context.Employees.ToList();
            var payrollService = new PayrollService();

            List<PayrollSummaryVm> list = new();

            foreach (var emp in employees)
            {
                var rows = _context.Attendances
                    .Where(a => a.Emp_Code == emp.EmployeeCode &&
                                a.Date.Year == y &&
                                a.Date.Month == m)
                    .ToList();

                if (!rows.Any())
                    continue;

                var summary = payrollService.CalculatePayroll(emp, rows, y, m);
                list.Add(summary);
            }

            ViewBag.Year = y;
            ViewBag.Month = m;

            return View(list); // Monthly.cshtml
        }

        // ================= SALARY SLIP =================
        public IActionResult Payslip(string empCode, int year, int month)
        {
            var emp = _context.Employees
                .FirstOrDefault(e => e.EmployeeCode == empCode);

            if (emp == null)
                return NotFound("Employee not found");

            var rows = _context.Attendances
                .Where(a => a.Emp_Code == empCode &&
                            a.Date.Year == year &&
                            a.Date.Month == month)
                .ToList();

            if (!rows.Any())
                return NotFound("No attendance data for this month.");

            var payrollService = new PayrollService();
            var summary = payrollService.CalculatePayroll(emp, rows, year, month);

            return View(summary); // Payslip.cshtml
        }
    }
}
