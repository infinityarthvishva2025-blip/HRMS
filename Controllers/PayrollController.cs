using HRMS.Data;
using HRMS.Models;
using HRMS.Models.ViewModels;
using HRMS.Services;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;

namespace HRMS.Controllers
{
    public class PayrollController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PayrollService _payrollService;

        public PayrollController(ApplicationDbContext context)
        {
            _context = context;
            _payrollService = new PayrollService(context);
        }

        // ============================================================
        // EMPLOYEE — DOWNLOAD SALARY SLIP (PDF)
        // ============================================================
        public IActionResult DownloadSalarySlip(int month, int year, string empCode)
        {
            var data = _payrollService.BuildMonthlySummary(empCode, year, month);

            if (data == null)
                return Content("Salary slip not found.");

            byte[] pdf = GenerateSalarySlipPdf(data);

            return File(
                pdf,
                "application/pdf",
                $"{empCode}_{month}_{year}_SalarySlip.pdf"
            );
        }

        // ============================================================
        // PDF GENERATION (iTextSharp)
        // ============================================================
        private byte[] GenerateSalarySlipPdf(PayrollSummaryVm m)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Document doc = new Document(PageSize.A4, 40, 40, 40, 40);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                // FONTS
                var titleFont = FontFactory.GetFont("Arial", 18, Font.BOLD);
                var headerFont = FontFactory.GetFont("Arial", 12, Font.BOLD);
                var boldFont = FontFactory.GetFont("Arial", 11, Font.BOLD);
                var normalFont = FontFactory.GetFont("Arial", 11);

                // ============================================================
                // COMPANY HEADER
                // ============================================================
                PdfPTable headerTable = new PdfPTable(2);
                headerTable.WidthPercentage = 100;
                headerTable.SetWidths(new float[] { 30, 70 });

                string logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/infinity-logo.png");
                if (System.IO.File.Exists(logoPath))
                {
                    iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(logoPath);
                    logo.ScaleAbsolute(80f, 55f);
                    PdfPCell logoCell = new PdfPCell(logo)
                    {
                        Border = Rectangle.NO_BORDER,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    headerTable.AddCell(logoCell);
                }
                else
                {
                    headerTable.AddCell(new PdfPCell(new Phrase("Infinity Arthvishva", boldFont)) { Border = Rectangle.NO_BORDER });
                }

                PdfPCell companyCell = new PdfPCell { Border = Rectangle.NO_BORDER };
                companyCell.AddElement(new Paragraph("Infinity Arthvishva", titleFont));
                companyCell.AddElement(new Paragraph("Empowering Finance & Innovation", normalFont));
                headerTable.AddCell(companyCell);
                doc.Add(headerTable);

                // ============================================================
                // TITLE
                // ============================================================
                doc.Add(new Paragraph("\nSALARY SLIP", titleFont) { Alignment = Element.ALIGN_CENTER });
                doc.Add(new Paragraph($"Month: {new DateTime(m.Year, m.Month, 1):MMMM yyyy}\n\n", boldFont));

                // ============================================================
                // EMPLOYEE INFORMATION
                // ============================================================
                PdfPTable empTable = new PdfPTable(2);
                empTable.WidthPercentage = 100;
                empTable.SetWidths(new float[] { 40, 60 });

                void AddRow(PdfPTable t, string label, string value)
                {
                    t.AddCell(new PdfPCell(new Phrase(label, boldFont)) { Border = Rectangle.NO_BORDER });
                    t.AddCell(new PdfPCell(new Phrase(value, normalFont)) { Border = Rectangle.NO_BORDER });
                }

                AddRow(empTable, "Employee Code:", m.EmpCode);
                AddRow(empTable, "Name:", m.EmpName);
                AddRow(empTable, "Department:", m.Department);
                AddRow(empTable, "Designation:", m.Designation);
                doc.Add(new Paragraph("\nEmployee Information", headerFont));
                doc.Add(empTable);

                // ============================================================
                // BANK INFORMATION
                // ============================================================
                PdfPTable bankTable = new PdfPTable(2);
                bankTable.WidthPercentage = 100;
                bankTable.SetWidths(new float[] { 40, 60 });

                AddRow(bankTable, "Bank Name:", m.BankName);
                AddRow(bankTable, "Account No:", m.AccountNumber);
                AddRow(bankTable, "IFSC Code:", m.IFSCCode);
                AddRow(bankTable, "Branch:", m.BankBranch);
                doc.Add(new Paragraph("\nBank Information", headerFont));
                doc.Add(bankTable);

                // ============================================================
                // ATTENDANCE SUMMARY
                // ============================================================
                PdfPTable attTable = new PdfPTable(9);
                attTable.WidthPercentage = 100;
                attTable.SetWidths(new float[] { 11, 11, 11, 11, 11, 11, 11, 11, 12 });

                void AddAtt(string text, bool header = false)
                {
                    attTable.AddCell(new PdfPCell(new Phrase(text, header ? boldFont : normalFont))
                    {
                        BackgroundColor = header ? BaseColor.LIGHT_GRAY : BaseColor.WHITE,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    });
                }

                // Calculate Leave & Late Loss
                decimal halfDayLoss = m.PresentHalfDays * 0.5m * m.PerDaySalary;
                decimal absentLoss = m.AbsentDays * m.PerDaySalary;
                decimal totalLeaveLoss = halfDayLoss + absentLoss;
                decimal lateLoss = m.LateDeductionDays * m.PerDaySalary;

                AddAtt("Work Days", true);
                AddAtt("Half Days", true);
                AddAtt("WO", true);
                AddAtt("Sat(WOP)", true);
                AddAtt("Absent", true);
                AddAtt("Late Marks", true);
                AddAtt("Late Ded (Days)", true);
                AddAtt("Paid Days", true);
                AddAtt("Leave Loss (₹)", true);

                AddAtt(m.TotalDaysInMonth.ToString());
                AddAtt(m.PresentHalfDays.ToString());
                AddAtt(m.WeeklyOffDays.ToString());
                AddAtt(m.TotalSaturdayPaid.ToString());
                AddAtt(m.AbsentDays.ToString());
                AddAtt(m.LateMarks.ToString());
                AddAtt(m.LateDeductionDays.ToString("0.0"));
                AddAtt(m.PaidDays.ToString("0.0"));
                AddAtt(totalLeaveLoss.ToString("0.00"));

                doc.Add(new Paragraph("\nAttendance Summary", headerFont));
                doc.Add(attTable);

                // ============================================================
                // EARNINGS SECTION
                // ============================================================
                PdfPTable earnTable = new PdfPTable(2);
                earnTable.WidthPercentage = 100;
                earnTable.SetWidths(new float[] { 60, 40 });

                void AddEarn(string lbl, decimal val, bool highlight = false)
                {
                    earnTable.AddCell(new PdfPCell(new Phrase(lbl, boldFont)) { Border = Rectangle.NO_BORDER });
                    earnTable.AddCell(new PdfPCell(new Phrase(val.ToString("0.00"), highlight ? boldFont : normalFont))
                    {
                        Border = Rectangle.NO_BORDER,
                        BackgroundColor = highlight ? new BaseColor(220, 240, 255) : BaseColor.WHITE,
                        HorizontalAlignment = Element.ALIGN_RIGHT
                    });
                }

                AddEarn("Base Salary", m.MonthlySalary);
                AddEarn("Per Day Salary", m.PerDaySalary);
                AddEarn("Performance Allowance", m.PerformanceAllowance);
                AddEarn("Other Allowances", m.OtherAllowances);
                AddEarn("Petrol Allowance", m.PetrolAllowance);
                AddEarn("Reimbursement", m.Reimbursement);
                AddEarn("Gross Salary", m.GrossSalary, true);

                doc.Add(new Paragraph("\nEarnings", headerFont));
                doc.Add(earnTable);

                // ============================================================
                // DEDUCTIONS SECTION
                // ============================================================
                PdfPTable dedTable = new PdfPTable(2);
                dedTable.WidthPercentage = 100;
                dedTable.SetWidths(new float[] { 60, 40 });

                void AddDed(string lbl, decimal val, bool red = false, bool green = false)
                {
                    BaseColor bg = BaseColor.WHITE;
                    if (red) bg = new BaseColor(255, 225, 225);
                    if (green) bg = new BaseColor(225, 255, 225);

                    dedTable.AddCell(new PdfPCell(new Phrase(lbl, boldFont)) { Border = Rectangle.NO_BORDER });
                    dedTable.AddCell(new PdfPCell(new Phrase(val.ToString("0.00"), boldFont))
                    {
                        Border = Rectangle.NO_BORDER,
                        BackgroundColor = bg,
                        HorizontalAlignment = Element.ALIGN_RIGHT
                    });
                }

                AddDed("Professional Tax", m.ProfessionalTax);
                AddDed("Leave Loss (Absents + Half-Days)", totalLeaveLoss, true);
                AddDed("Late Deduction (Days × Per Day)", lateLoss, true);
                AddDed("Other Deductions", m.OtherDeductions, true);
                AddDed("Total Deductions", m.TotalDeductions + lateLoss, true);
                AddDed("Net Salary", m.NetSalary - lateLoss, false, true);
                AddDed("Total Payable", m.TotalPay, false, true);

                doc.Add(new Paragraph("\nDeductions", headerFont));
                doc.Add(dedTable);

                // ============================================================
                // FOOTER
                // ============================================================
                doc.Add(new Paragraph("\nNotes:", headerFont));
                doc.Add(new Paragraph(
                    "• Saturday (WOP) is considered a paid day.\n" +
                    "• From the 4th late mark onward, each late = 0.5-day deduction.\n" +
                    "• Leave Loss = (Absents + Half Days × 0.5) × Per Day Salary.\n", normalFont));

                doc.Add(new Paragraph("\n*This is a system-generated salary slip and does not require a signature.", normalFont));
                doc.Add(new Paragraph("Generated on: " + DateTime.Now.ToString("dd-MM-yyyy HH:mm"), normalFont));

                doc.Close();
                return ms.ToArray();
            }
        }

        // ============================================================
        // ADMIN — MONTHLY PAYROLL
        // ============================================================
        public IActionResult Monthly(int? year, int? month, string search)
        {
            int y = year ?? DateTime.Now.Year;
            int m = month ?? DateTime.Now.Month;

            ViewBag.Year = y;
            ViewBag.Month = m;
            ViewBag.Search = search;

            var list = _payrollService.GetMonthlySummaries(y, m);

            // SEARCH
            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = search.ToLower();
                list = list
                    .Where(x =>
                        (x.EmpCode != null && x.EmpCode.ToLower().Contains(s)) ||
                        (x.EmpName != null && x.EmpName.ToLower().Contains(s)))
                    .ToList();
            }

            return View(list);
        }

        // ============================================================
        // ADMIN — VIEW PAYSLIP
        // ============================================================
        public IActionResult Payslip(string empCode, int year, int month)
        {
            var vm = _payrollService.BuildMonthlySummary(empCode, year, month);

            if (vm == null)
                return Content("Salary slip not found.");

            return View(vm);
        }
    }
}
