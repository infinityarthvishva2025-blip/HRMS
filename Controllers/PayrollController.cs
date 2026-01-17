using HRMS.Data;
using HRMS.Models.ViewModels;
using HRMS.Services;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.IO;
using System.Linq;
using System.Text;


namespace HRMS.Controllers
{
    public class PayrollController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PayrollService _payrollService;
        private readonly IRazorViewEngine _viewEngine;
        private readonly ITempDataDictionaryFactory _tempDataFactory;

        public PayrollController(
            ApplicationDbContext context,
            IRazorViewEngine viewEngine,
            ITempDataDictionaryFactory tempDataFactory)
        {
            _context = context;
            _payrollService = new PayrollService(context);
            _viewEngine = viewEngine;
            _tempDataFactory = tempDataFactory;
        }

        // ============================================================
        // ADMIN — MONTHLY PAYROLL LIST
        // ============================================================
        public IActionResult Monthly(int? year, int? month, string search)
        {
            int y = year ?? DateTime.Now.Year;
            int m = month ?? DateTime.Now.Month;

            ViewBag.Year = y;
            ViewBag.Month = m;
            ViewBag.Search = search;

            var list = _payrollService.GetMonthlySummaries(y, m);

            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = search.ToLower();
                list = list.Where(x =>
                    x.EmpCode.ToLower().Contains(s) ||
                    x.EmpName.ToLower().Contains(s)
                ).ToList();
            }

            return View(list);
        }

        // ============================================================
        // VIEW PAYSLIP — MONTHLY (HTML)
        // ============================================================
        [HttpGet]
        public IActionResult Payslip(string empCode, int year, int month)
        {
            var vm = _payrollService.BuildMonthlySummary(empCode, year, month);
            if (vm == null) return Content("Salary slip not found");

            return View("Payslip", vm);
        }

        // ============================================================
        // DOWNLOAD PAYSLIP — MONTHLY (PDF)
        // ============================================================
        [HttpGet]
        public IActionResult DownloadSalarySlip(string empCode, int year, int month)
        {
            var vm = _payrollService.BuildMonthlySummary(empCode, year, month);
            if (vm == null) return Content("Salary slip not found");

            return File(
                GenerateSalarySlipPdf(vm),
                "application/pdf",
                $"{empCode}_{month}_{year}_SalarySlip.pdf"
            );
        }

        // ============================================================
        // GENERATE PAYROLL — DATE RANGE
        // ============================================================
        [HttpGet]
        public IActionResult Generate()
        {
            return View(new PayrollViewModel
            {
                FromDate = DateTime.Today,
                ToDate = DateTime.Today
            });
        }

        [HttpPost]
        public IActionResult Generate(PayrollViewModel model)
        {
            if (model.FromDate > model.ToDate)
            {
                ModelState.AddModelError("", "Invalid date range");
                return View(model);
            }

            var empCodes = _context.Employees
     .Where(e => e.Status == "Active")
     .Select(e => e.EmployeeCode)
     .ToList();

            var list = empCodes
                .Select(code =>
                    _payrollService.BuildPayrollByDateRange(
                        code,
                        model.FromDate.Date,
                        model.ToDate.Date))
                .Where(x => x != null)
                .ToList();

            ViewBag.FromDate = model.FromDate;
            ViewBag.ToDate = model.ToDate;

            return View("PayrollList", list);
        }

        // ============================================================
        // VIEW PAYSLIP — DATE RANGE (HTML)
        // ============================================================
        [HttpGet]
        public IActionResult PayslipByDateRange(string empCode, DateTime fromDate, DateTime toDate)
        {
            var vm = _payrollService.BuildPayrollByDateRange(empCode, fromDate, toDate);
            if (vm == null) return Content("Salary slip not found");

            return View("Payslip", vm);
        }

        // ============================================================
        // DOWNLOAD PAYSLIP — DATE RANGE (PDF)
        // ============================================================
        [HttpGet]
        public IActionResult DownloadSalarySlipByDateRange(string empCode, DateTime fromDate, DateTime toDate)
        {
            var vm = _payrollService.BuildPayrollByDateRange(empCode, fromDate, toDate);
            if (vm == null) return Content("Salary slip not found");

            return File(
                GenerateSalarySlipPdf(vm),
                "application/pdf",
                $"{empCode}_{fromDate:ddMMyyyy}_{toDate:ddMMyyyy}_SalarySlip.pdf"
            );
        }

        // ============================================================
        // RENDER RAZOR VIEW → STRING
        // ============================================================
        private string RenderViewToString(string viewName, object model)
        {
            ViewData.Model = model;

            using var sw = new StringWriter();

            var viewResult = _viewEngine.FindView(
                ControllerContext,
                viewName,
                false);

            if (viewResult.View == null)
                throw new Exception($"View {viewName} not found");

            var viewContext = new ViewContext(
                ControllerContext,
                viewResult.View,
                ViewData,
                _tempDataFactory.GetTempData(HttpContext),
                sw,
                new HtmlHelperOptions()
            );

            viewResult.View.RenderAsync(viewContext).GetAwaiter().GetResult();
            return sw.ToString();
        }

        // ============================================================
        // PDF GENERATION — RAZOR BASED
        // ============================================================
        private byte[] GenerateSalarySlipPdf(PayrollSummaryVm model)
        {
            using var ms = new MemoryStream();

            string html = RenderViewToString("Payslip", model);

            using var doc = new Document(PageSize.A4, 30, 30, 30, 30);
            var writer = PdfWriter.GetInstance(doc, ms);
            doc.Open();

            using var sr = new StringReader(html);
            XMLWorkerHelper.GetInstance().ParseXHtml(writer, doc, sr);

            doc.Close();
            return ms.ToArray();
        }


    }
}
