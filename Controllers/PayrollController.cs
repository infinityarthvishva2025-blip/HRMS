using HRMS.Data;
using HRMS.Helpers;
using HRMS.Models;
using HRMS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
public class PayrollController : Controller
{
    private readonly IPayrollService _payrollService;
    private readonly ApplicationDbContext _context;

    public PayrollController(IPayrollService payrollService, ApplicationDbContext context)
    {
        _payrollService = payrollService;
        _context = context;
    }
    // =========================
    // GET : Generate Payroll
    // =========================
    [HttpGet]
    public IActionResult Generate()
    {
        var model = new PayrollViewModel
        {
            FromDate = DateTime.Today,
            ToDate = DateTime.Today,
            Month = DateTime.Today.Month,
            Year = DateTime.Today.Year
        };

        return View(model);
    }

    // =========================
    // POST : Generate Payroll
    // =========================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Generate(PayrollViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        await _payrollService.GeneratePayrollAsync(
            model.Month,
            model.Year,
            model.FromDate,
            model.ToDate
        );

        return RedirectToAction("PayrollList",
            new { month = model.Month, year = model.Year });
    }

    // ======================
    // GENERATE PAYROLL
    // ======================
    //[HttpPost]
    //public async Task<IActionResult> Generate(PayrollViewModel model)
    //{
    //    if (_payrollService.IsPayrollLocked(model.Month, model.Year))
    //    {
    //        TempData["Error"] = "Payroll already locked!";
    //        return RedirectToAction("PayrollList", new { model.Month, model.Year });
    //    }

    //    await _payrollService.GeneratePayrollAsync(
    //        model.Month, model.Year, model.FromDate, model.ToDate);

    //    return RedirectToAction("PayrollList", new { model.Month, model.Year });
    //}

    // ======================
    // LOCK PAYROLL
    // ======================
    [HttpPost]
    public IActionResult LockPayroll(int month, int year)
    {
        _payrollService.LockPayroll(month, year, User.Identity.Name);
        return RedirectToAction("PayrollList", new { month, year });
    }

    // ======================
    // PAYROLL LIST
    // ======================
    [HttpGet]
    public IActionResult PayrollList(int month, int year, int page = 1)
    {
        //int pageSize = 10;

        bool isLocked = _context.PayrollLocks
            .Any(x => x.Month == month && x.Year == year && x.IsLocked);

        ViewBag.IsLocked = isLocked;
        ViewBag.Month = month;
        ViewBag.Year = year;

        int totalRecords = _context.Payroll
            .Count(x => x.month == month && x.year == year);

        var data = _context.Payroll
            .Where(x => x.month == month && x.year == year)
            .OrderBy(x => x.emp_code)
            //.Skip((page - 1) * pageSize)
            //.Take(pageSize)
            .ToList();

        ViewBag.CurrentPage = page;
       // ViewBag.TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        return View(data);
    }

    public IActionResult ExportExcel(int month, int year)
    {
        var data = _context.Payroll
            .Where(x => x.month == month && x.year == year)
            .AsNoTracking()
            .ToList();

        var file = PayrollExportHelper.ExportToExcel(data);

        return File(
            file,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "Payroll.xlsx"
        );
    }

    [HttpGet]
    public IActionResult Payslip(string token)
    {
        if (string.IsNullOrEmpty(token))
            return NotFound();

        var payroll = _context.Payroll
            .FirstOrDefault(x => x.emp_code == token);

        if (payroll == null)
            return NotFound();

        return View(payroll);
    }
    [HttpGet]
    public IActionResult PayslipPdf(string token)
    {
        if (string.IsNullOrEmpty(token))
            return NotFound();

        var payroll = _context.Payroll
            .FirstOrDefault(x => x.emp_code == token);

        if (payroll == null)
            return NotFound();

        return new ViewAsPdf("Payslip", payroll)
        {
            FileName = $"Payslip_{payroll.emp_code}.pdf"
        };
    }
    public IActionResult PayslipPdf(int id)
    {
        var payroll = _context.Payroll.Find(id);
        return new ViewAsPdf("Payslip", payroll)
        {
            FileName = "Payslip.pdf"
        };
    }

}


//using HRMS.Data;
//using HRMS.Models;
//using HRMS.Services;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace HRMS.Controllers
//{
//    public class PayrollController : Controller
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly PayrollDeductionService _deductionService;
//        private readonly PayslipEmailService _emailService;
//        private readonly SalarySlipService _salarySlipService;

//        public PayrollController(
//            ApplicationDbContext context,
//            PayrollDeductionService deductionService,
//            PayslipEmailService emailService,
//            SalarySlipService salarySlipService)
//        {
//            _context = context;
//            _deductionService = deductionService;
//            _emailService = emailService;
//            _salarySlipService = salarySlipService;
//        }

//        // =========================================================
//        // ✅ HR APPROVE
//        // =========================================================
//        [HttpPost]
//        public IActionResult HrApprove(int id, string? remark)
//        {
//            var payroll = _context.Payrolls.Include(p => p.Employee).FirstOrDefault(p => p.Id == id);
//            if (payroll == null) return NotFound();

//            if (payroll.IsLocked) return BadRequest("Payroll is locked.");

//            payroll.HrStatus = "Approved";
//            payroll.HrRemark = remark;
//            payroll.OverallStatus = "Pending";

//            _context.SaveChanges();
//            return RedirectToAction("Index");
//        }

//        [HttpPost]
//        public IActionResult HrReject(int id, string? remark)
//        {
//            var payroll = _context.Payrolls.Include(p => p.Employee).FirstOrDefault(p => p.Id == id);
//            if (payroll == null) return NotFound();

//            if (payroll.IsLocked) return BadRequest("Payroll is locked.");

//            payroll.HrStatus = "Rejected";
//            payroll.HrRemark = remark;
//            payroll.OverallStatus = "Rejected";

//            _context.SaveChanges();
//            return RedirectToAction("Index");
//        }

//        // =========================================================
//        // ✅ DIRECTOR APPROVE (FINAL)
//        // =========================================================
//        [HttpPost]
//        public IActionResult DirectorApprove(int id, string? remark)
//        {
//            var payroll = _context.Payrolls.Include(p => p.Employee).FirstOrDefault(p => p.Id == id);
//            if (payroll == null) return NotFound();

//            if (payroll.IsLocked) return BadRequest("Payroll is locked.");

//            // ✅ MUST BE HR APPROVED FIRST
//            if (payroll.HrStatus != "Approved")
//                return BadRequest("HR approval required first.");

//            payroll.DirectorStatus = "Approved";
//            payroll.DirectorRemark = remark;

//            // ✅ FINAL STATUS
//            payroll.OverallStatus = "Approved";
//            payroll.IsLocked = true;

//            // ✅ Recalculate PF/ESI + totals safely before finalizing
//            _deductionService.ApplyPFESI(payroll);

//            _context.SaveChanges();

//            // ✅ AUTO SEND PAYSLIP EMAIL
//            TrySendPayslipEmail(payroll);

//            return RedirectToAction("Index");
//        }

//        [HttpPost]
//        public IActionResult DirectorReject(int id, string? remark)
//        {
//            var payroll = _context.Payrolls.Include(p => p.Employee).FirstOrDefault(p => p.Id == id);
//            if (payroll == null) return NotFound();

//            if (payroll.IsLocked) return BadRequest("Payroll is locked.");

//            payroll.DirectorStatus = "Rejected";
//            payroll.DirectorRemark = remark;
//            payroll.OverallStatus = "Rejected";

//            _context.SaveChanges();
//            return RedirectToAction("Index");
//        }

//        // =========================================================
//        // ✅ PAYSLIP EMAIL SENDER (Internal)
//        // =========================================================
//        private void TrySendPayslipEmail(Payroll payroll)
//        {
//            if (payroll.PayslipEmailSent) return;
//            if (string.IsNullOrWhiteSpace(payroll.Employee.Email)) return;

//            var pdf = _salarySlipService.Generate(payroll);

//            string subject = $"Salary Slip - {payroll.Month}/{payroll.Year}";
//            string body = $@"
//                <p>Hello <b>{payroll.Employee.Name}</b>,</p>
//                <p>Your salary slip for <b>{payroll.Month}/{payroll.Year}</b> is attached.</p>
//                <p>Net Salary: <b>₹{payroll.NetSalary}</b></p>
//                <p>Regards,<br/>HRMS Payroll</p>";

//            _emailService.SendPayslip(
//                payroll.Employee.Email,
//                payroll.Employee.Name,
//                subject,
//                body,
//                pdf,
//                $"SalarySlip_{payroll.Employee.EmployeeCode}_{payroll.Month}_{payroll.Year}.pdf"
//            );

//            payroll.PayslipEmailSent = true;
//            _context.SaveChanges();
//        }
//    }
//}
