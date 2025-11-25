using HRMS.Data;
using HRMS.Models;
using HRMS.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HRMS.Controllers
{
    public class PayrollController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PayrollController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // EMPLOYEE — DOWNLOAD SALARY SLIP
        // ============================================================
        public IActionResult DownloadSalarySlip(int month, int year)
        {
            string monthName = new DateTime(year, month, 1)
                                .ToString("MMMM")
                                .ToUpper();

            string empCode = HttpContext.Session.GetString("EmployeeCode");

            if (string.IsNullOrEmpty(empCode))
                return Content("Session expired. Please login again.");

            var salary = _context.Payroll
                .FirstOrDefault(x =>
                    x.emp_code == empCode &&
                    x.month.ToUpper() == monthName
                );

            if (salary == null)
                return Content($"Salary slip for {monthName}/{year} not found.");

            byte[] pdf = GenerateSalarySlipPdf(salary, year);

            return File(
                pdf,
                "application/pdf",
                $"{empCode}_{monthName}_{year}_SalarySlip.pdf"
            );
        }

        // ============================================================
        // PDF USING iTextSharp
        // ============================================================
        private byte[] GenerateSalarySlipPdf(Payroll salary, int year)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Document doc = new Document(PageSize.A4, 40, 40, 40, 40);
                PdfWriter.GetInstance(doc, ms);

                doc.Open();

                Font titleFont = FontFactory.GetFont("Arial", 20, Font.BOLD);

                Paragraph heading = new Paragraph("SALARY SLIP", titleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20
                };
                doc.Add(heading);

                Font labelFont = FontFactory.GetFont("Arial", 12, Font.BOLD);
                Font valueFont = FontFactory.GetFont("Arial", 12);

                PdfPTable table = new PdfPTable(2);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 40, 60 });

                void AddRow(string label, string value)
                {
                    table.AddCell(new PdfPCell(new Phrase(label, labelFont))
                    { Border = Rectangle.NO_BORDER });

                    table.AddCell(new PdfPCell(new Phrase(value, valueFont))
                    { Border = Rectangle.NO_BORDER });
                }

                AddRow("Employee Code:", salary.emp_code);
                AddRow("Employee Name:", salary.name ?? "-");
                AddRow("Month:", salary.month);
                AddRow("Year:", year.ToString());

                AddRow("Working Days:", (salary.working_days ?? 0).ToString());
                AddRow("Leaves Taken:", (salary.leaves_taken ?? 0).ToString());
                AddRow("Late Marks:", (salary.late_marks ?? 0).ToString());
                AddRow("Late Deduction Days:", (salary.late_deduction_days ?? 0).ToString());
                AddRow("Paid Days:", (salary.paid_days ?? 0).ToString());

                AddRow("Base Salary:", (salary.base_salary ?? 0).ToString("0.00"));
                AddRow("Performance Allowance:", (salary.perf_allowance ?? 0).ToString("0.00"));
                AddRow("Other Allowance:", (salary.other_allowance ?? 0).ToString("0.00"));
                AddRow("Petrol Allowance:", (salary.petrol_allowance ?? 0).ToString("0.00"));
                AddRow("Reimbursement:", (salary.reimbursement ?? 0).ToString("0.00"));

                AddRow("Gross Salary:", (salary.gross_salary ?? 0).ToString("0.00"));
                AddRow("Professional Tax:", (salary.prof_tax ?? 0).ToString("0.00"));
                AddRow("Total Deduction:", (salary.total_deduction ?? 0).ToString("0.00"));
                AddRow("Net Salary:", (salary.net_salary ?? 0).ToString("0.00"));
                AddRow("Total Pay:", (salary.total_pay ?? 0).ToString("0.00"));

                doc.Add(table);
                doc.Close();

                return ms.ToArray();
            }
        }

        // ============================================================
        // ADMIN — MONTHLY PAYROLL
        // ============================================================
        public IActionResult Monthly(int? year, int? month)
        {
            int y = year ?? DateTime.Now.Year;
            int m = month ?? DateTime.Now.Month;

            ViewBag.Year = y;
            ViewBag.Month = m;

            string monthName = new DateTime(y, m, 1)
                                .ToString("MMMM")
                                .ToUpper();

            var saved = _context.Payroll
               .Where(p => p.month.ToUpper() == monthName)
               .ToList();

            var vmList = saved.Select(p => new PayrollSummaryVm
            {
                EmpCode = p.emp_code,
                EmpName = p.name,

                Year = y,
                Month = m,

                TotalDaysInMonth = p.working_days ?? 0,
                AbsentDays = p.leaves_taken ?? 0,
                LateMarks = p.late_marks ?? 0,
                LateDeductionDays = p.late_deduction_days ?? 0,
                PaidDays = p.paid_days ?? 0,

                MonthlySalary = p.base_salary ?? 0,
                PerDaySalary = p.per_day_salary ?? 0,
                GrossSalary = p.gross_salary ?? 0,

                PerformanceAllowance = p.perf_allowance ?? 0,
                OtherAllowances = p.other_allowance ?? 0,
                PetrolAllowance = p.petrol_allowance ?? 0,
                Reimbursement = p.reimbursement ?? 0,

                ProfessionalTax = p.prof_tax ?? 0,
                TotalDeductions = p.total_deduction ?? 0,
                NetSalary = p.net_salary ?? 0,
                TotalPay = p.total_pay ?? 0

            }).ToList();

            return View(vmList);
        }

        // ============================================================
        // ADMIN — VIEW PAYSLIP
        // ============================================================
        public IActionResult Payslip(string empCode, int year, int month)
        {
            if (string.IsNullOrEmpty(empCode))
                return Content("Employee code missing.");

            string monthName = new DateTime(year, month, 1)
                .ToString("MMMM").ToUpper();

            var p = _context.Payroll.FirstOrDefault(x =>
                x.emp_code == empCode &&
                x.month.ToUpper() == monthName
            );

            if (p == null)
                return Content("Salary slip not found.");

            var vm = new PayrollSummaryVm
            {
                EmpCode = p.emp_code,
                EmpName = p.name,
                Year = year,
                Month = month,

                TotalDaysInMonth = p.working_days ?? 0,
                AbsentDays = p.leaves_taken ?? 0,
                LateMarks = p.late_marks ?? 0,
                LateDeductionDays = p.late_deduction_days ?? 0,
                PaidDays = p.paid_days ?? 0,

                MonthlySalary = p.base_salary ?? 0,
                PerDaySalary = p.per_day_salary ?? 0,
                GrossSalary = p.gross_salary ?? 0,
                PerformanceAllowance = p.perf_allowance ?? 0,
                OtherAllowances = p.other_allowance ?? 0,
                PetrolAllowance = p.petrol_allowance ?? 0,
                Reimbursement = p.reimbursement ?? 0,

                ProfessionalTax = p.prof_tax ?? 0,
                TotalDeductions = p.total_deduction ?? 0,
                NetSalary = p.net_salary ?? 0,
                TotalPay = p.total_pay ?? 0
            };

            return View(vm);
        }
    }
}
