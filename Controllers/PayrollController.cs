using HRMS.Data;
using HRMS.Models;
using HRMS.Models.ViewModels;
using HRMS.Services;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;

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
        // EMPLOYEE — DOWNLOAD SALARY SLIP (iTextSharp PDF)
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
        // PDF USING iTextSharp
        // ============================================================
        private byte[] GenerateSalarySlipPdf(PayrollSummaryVm m)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Document doc = new Document(PageSize.A4, 40, 40, 40, 40);
                PdfWriter writer = PdfWriter.GetInstance(doc, ms);
                doc.Open();

                // Fonts
                var titleFont = FontFactory.GetFont("Arial", 18, Font.BOLD);
                var headerFont = FontFactory.GetFont("Arial", 12, Font.BOLD);
                var boldFont = FontFactory.GetFont("Arial", 11, Font.BOLD);
                var normalFont = FontFactory.GetFont("Arial", 11);

                // ============================================================
                // LOGO + COMPANY NAME
                // ============================================================
                PdfPTable logoTable = new PdfPTable(2);
                logoTable.WidthPercentage = 100;
                logoTable.SetWidths(new float[] { 30, 70 });

                string imgPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/infinity-logo.png");

                if (System.IO.File.Exists(imgPath))
                {
                    iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(imgPath);
                    logo.ScaleAbsolute(80f, 55f);

                    PdfPCell logoCell = new PdfPCell(logo)
                    {
                        Border = Rectangle.NO_BORDER,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    logoTable.AddCell(logoCell);
                }
                else
                {
                    logoTable.AddCell(new PdfPCell(new Phrase("Infinity Logo", boldFont)) { Border = Rectangle.NO_BORDER });
                }

                PdfPCell compCell = new PdfPCell
                {
                    Border = Rectangle.NO_BORDER
                };
                compCell.AddElement(new Paragraph("Infinity Arthvishva", titleFont));
                compCell.AddElement(new Paragraph("Empowering Finance & Innovation", normalFont));
                logoTable.AddCell(compCell);

                doc.Add(logoTable);

                // Title
                doc.Add(new Paragraph("\nSALARY SLIP", titleFont) { Alignment = Element.ALIGN_CENTER });
                doc.Add(new Paragraph($"Month: {new DateTime(m.Year, m.Month, 1):MMMM yyyy}\n\n", boldFont));

                // ============================================================
                // EMPLOYEE INFORMATION
                // ============================================================
                PdfPTable empTable = new PdfPTable(2);
                empTable.WidthPercentage = 100;
                empTable.SetWidths(new float[] { 40, 60 });

                void AddEmpRow(string lbl, string val)
                {
                    empTable.AddCell(new PdfPCell(new Phrase(lbl, boldFont)));
                    empTable.AddCell(new PdfPCell(new Phrase(val, normalFont)));
                }

                AddEmpRow("Employee Code:", m.EmpCode);
                AddEmpRow("Name:", m.EmpName);
                AddEmpRow("Department:", m.Department);
                AddEmpRow("Designation:", m.Designation);

                doc.Add(new Paragraph("\nEmployee Information", headerFont));
                doc.Add(empTable);

                // ============================================================
                // BANK INFORMATION
                // ============================================================
                PdfPTable bankTable = new PdfPTable(2);
                bankTable.WidthPercentage = 100;
                bankTable.SetWidths(new float[] { 40, 60 });

                void AddBankRow(string lbl, string val)
                {
                    bankTable.AddCell(new PdfPCell(new Phrase(lbl, boldFont)));
                    bankTable.AddCell(new PdfPCell(new Phrase(val, normalFont)));
                }

                AddBankRow("Bank Name:", m.BankName);
                AddBankRow("Account No:", m.AccountNumber);
                AddBankRow("IFSC Code:", m.IFSCCode);
                AddBankRow("Branch:", m.BankBranch);

                doc.Add(new Paragraph("\nBank Information", headerFont));
                doc.Add(bankTable);

                // ============================================================
                // ATTENDANCE SUMMARY
                // ============================================================
                PdfPTable att = new PdfPTable(6);
                att.WidthPercentage = 100;
                att.SetWidths(new float[] { 15, 15, 15, 20, 15, 20 });

                void AddAttCell(string text, bool header = false)
                {
                    att.AddCell(new PdfPCell(new Phrase(text, header ? boldFont : normalFont))
                    {
                        BackgroundColor = header ? BaseColor.LIGHT_GRAY : BaseColor.WHITE,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    });
                }

                AddAttCell("Working Days", true);
                AddAttCell("Half Days", true);
                AddAttCell("Weekly Off", true);
                AddAttCell("Saturday (WOP)", true);
                AddAttCell("Absent", true);
                AddAttCell("Paid Days", true);

                AddAttCell(m.TotalDaysInMonth.ToString());
                AddAttCell(m.PresentHalfDays.ToString());
                AddAttCell(m.WeeklyOffDays.ToString());
                AddAttCell(m.TotalSaturdayPaid.ToString());
                AddAttCell(m.AbsentDays.ToString());
                AddAttCell(m.PaidDays.ToString());

                doc.Add(new Paragraph("\nAttendance Summary", headerFont));
                doc.Add(att);

                // ============================================================
                // EARNINGS
                // ============================================================
                PdfPTable earn = new PdfPTable(2);
                earn.WidthPercentage = 100;
                earn.SetWidths(new float[] { 60, 40 });

                void AddEarn(string lbl, decimal val, bool highlight = false)
                {
                    earn.AddCell(new PdfPCell(new Phrase(lbl, boldFont)));
                    earn.AddCell(new PdfPCell(new Phrase(val.ToString("0.00"), highlight ? boldFont : normalFont))
                    {
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
                doc.Add(earn);

                // ============================================================
                // DEDUCTIONS
                // ============================================================
                PdfPTable ded = new PdfPTable(2);
                ded.WidthPercentage = 100;
                ded.SetWidths(new float[] { 60, 40 });

                void AddDed(string lbl, decimal val, bool red = false, bool green = false)
                {
                    BaseColor bg = BaseColor.WHITE;
                    if (red) bg = new BaseColor(255, 225, 225);
                    if (green) bg = new BaseColor(225, 255, 225);

                    ded.AddCell(new PdfPCell(new Phrase(lbl, boldFont)));
                    ded.AddCell(new PdfPCell(new Phrase(val.ToString("0.00"), boldFont))
                    {
                        BackgroundColor = bg,
                        HorizontalAlignment = Element.ALIGN_RIGHT
                    });
                }

                AddDed("Professional Tax", m.ProfessionalTax);
                AddDed("Other Deductions", m.TotalDeductions - m.ProfessionalTax);
                AddDed("Total Deductions", m.TotalDeductions, true);
                AddDed("Net Salary", m.NetSalary, false, true);
                AddDed("Total Payable", m.TotalPay, false, true);

                doc.Add(new Paragraph("\nDeductions", headerFont));
                doc.Add(ded);

                // ============================================================
                // FOOTER
                // ============================================================
                doc.Add(new Paragraph("\n\n*This is a system-generated slip.", normalFont));

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

            // 🔍 SEARCH LOGIC
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
