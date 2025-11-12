using System;
using System.IO;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using HRMS.Data;       // <-- your DbContext namespace
using HRMS.Models;     // <-- your Payroll model namespace

namespace HRMS.Controllers
{
    public class PayrollController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PayrollController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Show all payroll records
        public IActionResult Index()
        {
            var payrolls = _context.Payrolls.ToList();
            return View(payrolls);
        }

        // Create payroll - GET
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // Create payroll - POST
        [HttpPost]
        public IActionResult Create(Payroll payroll)
        {
            if (ModelState.IsValid)
            {
                // compute totals on server to be safe (same logic you used earlier)
                payroll.TotalEarning = payroll.BaseSalary + payroll.PerformanceAllowance + payroll.OtherAllowances;
                payroll.TotalDeductions = payroll.ProfessionalTax + payroll.DeductionForLeaves;
                payroll.NetPay = payroll.TotalEarning - payroll.TotalDeductions;
                payroll.CreatedDate = DateTime.Now;

                _context.Payrolls.Add(payroll);
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }

            return View(payroll);
        }

        // GET: Generate Payslip PDF
        public IActionResult GeneratePayslip(int id)
        {
            var payroll = _context.Payrolls.FirstOrDefault(p => p.Id == id);
            if (payroll == null)
                return NotFound();

            using (MemoryStream ms = new MemoryStream())
            {
                Document doc = new Document(PageSize.A4, 40, 40, 40, 40);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                // Title
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 11);
                var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11);

                Paragraph header = new Paragraph("Infinity Arthvishva HRMS\nPayslip\n\n", titleFont);
                header.Alignment = Element.ALIGN_CENTER;
                doc.Add(header);

                // Employee Info Section
                PdfPTable empTable = new PdfPTable(2);
                empTable.WidthPercentage = 100;
                empTable.SpacingBefore = 10f;
                empTable.DefaultCell.Border = Rectangle.NO_BORDER;

                empTable.AddCell(new Phrase("Employee Name:", boldFont));
                empTable.AddCell(new Phrase(payroll.EmployeeName ?? "", normalFont));

                empTable.AddCell(new Phrase("Employee Code:", boldFont));
                empTable.AddCell(new Phrase(payroll.EmployeeCode ?? "", normalFont));

                empTable.AddCell(new Phrase("Designation:", boldFont));
                empTable.AddCell(new Phrase(payroll.Designation ?? "", normalFont));

                empTable.AddCell(new Phrase("PAN:", boldFont));
                empTable.AddCell(new Phrase(payroll.PAN ?? "", normalFont));

                empTable.AddCell(new Phrase("Bank Name:", boldFont));
                empTable.AddCell(new Phrase(payroll.BankName ?? "", normalFont));

                empTable.AddCell(new Phrase("Bank A/C No:", boldFont));
                empTable.AddCell(new Phrase(payroll.BankAccountNumber ?? "", normalFont));

                empTable.AddCell(new Phrase("Date of Joining:", boldFont));
                empTable.AddCell(new Phrase(payroll.DateOfJoining.ToString("dd-MMM-yyyy"), normalFont));

                empTable.AddCell(new Phrase("Salary Month & Year:", boldFont));
                empTable.AddCell(new Phrase(payroll.MonthYear ?? "", normalFont));

                doc.Add(empTable);
                doc.Add(new Paragraph("\n"));

                // Earnings Table
                PdfPTable earnTable = new PdfPTable(2);
                earnTable.WidthPercentage = 100;
                earnTable.AddCell(new Phrase("Earnings", boldFont));
                earnTable.AddCell(new Phrase("Amount (₹)", boldFont));

                earnTable.AddCell("Basic Salary");
                earnTable.AddCell(payroll.BaseSalary.ToString("N2"));

                earnTable.AddCell("Performance Allowance");
                earnTable.AddCell(payroll.PerformanceAllowance.ToString("N2"));

                earnTable.AddCell("Other Allowances");
                earnTable.AddCell(payroll.OtherAllowances.ToString("N2"));

                earnTable.AddCell("Total Earnings");
                earnTable.AddCell((payroll.BaseSalary + payroll.PerformanceAllowance + payroll.OtherAllowances).ToString("N2"));

                doc.Add(earnTable);
                doc.Add(new Paragraph("\n"));

                // Deductions Table
                PdfPTable dedTable = new PdfPTable(2);
                dedTable.WidthPercentage = 100;
                dedTable.AddCell(new Phrase("Deductions", boldFont));
                dedTable.AddCell(new Phrase("Amount (₹)", boldFont));

                dedTable.AddCell("Professional Tax");
                dedTable.AddCell(payroll.ProfessionalTax.ToString("N2"));

                dedTable.AddCell("Deduction for Leaves");
                dedTable.AddCell(payroll.DeductionForLeaves.ToString("N2"));

                dedTable.AddCell("Total Deductions");
                dedTable.AddCell((payroll.ProfessionalTax + payroll.DeductionForLeaves).ToString("N2"));

                doc.Add(dedTable);
                doc.Add(new Paragraph("\n"));

                // Attendance Summary
                PdfPTable attTable = new PdfPTable(2);
                attTable.WidthPercentage = 100;
                attTable.AddCell(new Phrase("Attendance Summary", boldFont));
                attTable.AddCell("");

                attTable.AddCell("Total Working Days");
                attTable.AddCell(payroll.TotalWorkingDays.ToString());

                attTable.AddCell("Days Attended");
                attTable.AddCell(payroll.DaysAttended.ToString());

                attTable.AddCell("Total Leaves Taken");
                attTable.AddCell(payroll.TotalLeavesTaken.ToString());

                doc.Add(attTable);
                doc.Add(new Paragraph("\n"));

                // Net Pay Section
                PdfPTable netTable = new PdfPTable(2);
                netTable.WidthPercentage = 100;
                netTable.AddCell(new Phrase("Net Pay", boldFont));
                netTable.AddCell(new Phrase("₹ " + payroll.NetPay.ToString("N2"), boldFont));

                doc.Add(netTable);

                // Footer
                doc.Add(new Paragraph("\n\nAuthorized Signature: ___________________________", normalFont));
                doc.Add(new Paragraph("Infinity Arthvishva HR Department", normalFont));

                doc.Close();
                byte[] bytes = ms.ToArray();
                return File(bytes, "application/pdf", $"{payroll.EmployeeName}_Payslip.pdf");
            }
        }
    }
}
