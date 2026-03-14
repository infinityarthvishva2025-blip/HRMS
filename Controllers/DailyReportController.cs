using HRMS.Data;
using HRMS.Models;
using HRMS.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HRMS.Controllers
{
    public class DailyReportController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly ApplicationDbContext _db;

        public DailyReportController(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // =========================
        // SESSION HELPERS
        // =========================
        private int GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("EmployeeId") ?? 0;
        }

        private string GetCurrentRole()
        {
            // Always normalize role so checks are consistent everywhere
            var role = HttpContext.Session.GetString("Role") ?? "";
            return NormalizeRole(role);
        }

        private string NormalizeRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role))
                return "Employee";

            role = role.Trim();

            // map admin -> HR
            if (role.Equals("admin", StringComparison.OrdinalIgnoreCase))
                return "HR";

            // ✅ FIX: HR was missing earlier
            if (role.Equals("hr", StringComparison.OrdinalIgnoreCase))
                return "HR";

            if (role.Equals("employee", StringComparison.OrdinalIgnoreCase))
                return "Employee";

            if (role.Equals("intern", StringComparison.OrdinalIgnoreCase))
                return "Intern";

            if (role.Equals("manager", StringComparison.OrdinalIgnoreCase))
                return "Manager";

            if (role.Equals("gm", StringComparison.OrdinalIgnoreCase))
                return "GM";

            if (role.Equals("vp", StringComparison.OrdinalIgnoreCase))
                return "VP";

            if (role.Equals("director", StringComparison.OrdinalIgnoreCase))
                return "Director";

            return "Employee";
        }

        // =========================
        // GET: DailyReport/Send
        // =========================
        public IActionResult Send()
        {
            string role = GetCurrentRole();

            // Directors cannot send reports
            if (role == "Director")
                return RedirectToAction("AccessDenied", "Account");

            int senderId = GetCurrentUserId();
            if (senderId == 0)
            {
                TempData["Error"] = "Session expired. Please login again.";
                return RedirectToAction("Login", "Account");
            }

            var sender = _db.Employees.FirstOrDefault(e => e.Id == senderId);
            if (sender == null)
            {
                TempData["Error"] = "Invalid user session. Please login again.";
                return RedirectToAction("Login", "Account");
            }

            var model = new DailyReportViewModel
            {
                RecipientList = BuildRecipientList(NormalizeRole(sender.Role))
            };

            return View(model);
        }

        // =========================
        // POST: DailyReport/Send
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Send(DailyReportViewModel model)
        {
            string role = GetCurrentRole();

            // Director cannot submit
            if (role == "Director")
                return RedirectToAction("AccessDenied", "Account");

            int senderId = GetCurrentUserId();
            if (senderId == 0)
            {
                TempData["Error"] = "Session expired. Please login again.";
                return RedirectToAction("Login", "Account");
            }

            var sender = _db.Employees.FirstOrDefault(e => e.Id == senderId);
            if (sender == null)
            {
                TempData["Error"] = "Invalid session.";
                return RedirectToAction("Login", "Account");
            }

            // Validate recipients
            if (model.SelectedRecipientIds == null || !model.SelectedRecipientIds.Any())
            {
                ModelState.AddModelError("SelectedRecipientIds", "Please select at least one recipient.");
            }

            if (!ModelState.IsValid)
            {
                model.RecipientList = BuildRecipientList(NormalizeRole(sender.Role));
                return View(model);
            }

            // =========================
            // FILE UPLOAD (OPTIONAL)
            // =========================
            string attachmentPath = null;

            if (model.Attachment != null && model.Attachment.Length > 0)
            {
                var ext = Path.GetExtension(model.Attachment.FileName).ToLower();

                if (!new[] { ".jpg", ".jpeg", ".png", ".pdf" }.Contains(ext))
                {
                    ModelState.AddModelError("Attachment", "Only JPG, PNG or PDF files are allowed.");
                    model.RecipientList = BuildRecipientList(NormalizeRole(sender.Role));
                    return View(model);
                }

                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "dailyreports");
                Directory.CreateDirectory(uploadsFolder);

                string fileName = $"{Guid.NewGuid()}{ext}";
                string fullPath = Path.Combine(uploadsFolder, fileName);

                using var stream = new FileStream(fullPath, FileMode.Create);
                model.Attachment.CopyTo(stream);

                attachmentPath = $"/uploads/dailyreports/{fileName}";
            }

            // =========================
            // SAVE REPORT
            // =========================
            var report = new DailyReport
            {
                SenderId = senderId,
                TodaysWork = model.TodaysWork,
                PendingWork = model.PendingWork,
                Issues = model.Issues,
                AttachmentPath = attachmentPath,
                CreatedDate = DateTime.Now
            };

            _db.DailyReports.Add(report);
            _db.SaveChanges();

            foreach (var receiverId in model.SelectedRecipientIds)
            {
                _db.DailyReportRecipients.Add(new DailyReportRecipient
                {
                    ReportId = report.ReportId,
                    ReceiverId = receiverId,
                    IsRead = false,
                    ReadDate = null
                });
            }

            _db.SaveChanges();

            // ===============================
            // AUTO CHECKOUT AFTER SUBMIT
            // ===============================
            var empCode = sender.EmployeeCode;

            if (!string.IsNullOrWhiteSpace(empCode))
            {
                var attendance = _db.Attendances.FirstOrDefault(a =>
                    a.Emp_Code == empCode &&
                    a.Date.Date == DateTime.Today &&
                    a.InTime != null &&
                    (a.OutTime == null || a.OutTime == TimeSpan.Zero)
                );

                if (attendance != null)
                {
                    // ===============================
                    // ✅ ADDED: FETCH GEO DATA FROM SESSION
                    // ===============================
                    var checkoutTimeStr = HttpContext.Session.GetString("CheckoutTime");
                    var checkoutLatStr = HttpContext.Session.GetString("CheckoutLat");
                    var checkoutLngStr = HttpContext.Session.GetString("CheckoutLng");

                    if (!string.IsNullOrEmpty(checkoutTimeStr) &&
                        TimeSpan.TryParse(checkoutTimeStr, out TimeSpan checkoutTime))
                    {
                        attendance.OutTime = checkoutTime;
                    }
                    else
                    {
                        attendance.OutTime = DateTime.Now.TimeOfDay;
                    }

                    if (double.TryParse(checkoutLatStr, out double lat))
                        attendance.CheckOutLatitude = lat;

                    if (double.TryParse(checkoutLngStr, out double lng))
                        attendance.CheckOutLongitude = lng;

                    attendance.IsGeoAttendance = true;

                    // ===============================
                    // EXISTING LOGIC (UNCHANGED)
                    // ===============================
                    attendance.Total_Hours =
                        Math.Round((decimal)(attendance.OutTime.Value - attendance.InTime.Value).TotalHours, 2);

                    attendance.Status = "P";

                    _db.SaveChanges();

                    // ===============================
                    // ✅ ADDED: CLEAR SESSION
                    // ===============================
                    HttpContext.Session.Remove("CheckoutTime");
                    HttpContext.Session.Remove("CheckoutLat");
                    HttpContext.Session.Remove("CheckoutLng");
                }
            }

            TempData["Success"] = "Daily report submitted successfully.";

            // ✅ CORRECT REDIRECT
            return RedirectToAction("EmployeePanel", "Attendance");
        }

        // =========================
        // GET: DailyReport/MySentReports
        // =========================
        public IActionResult MySentReports(DateTime? selectedDate)
        {
            int senderId = GetCurrentUserId();
            if (senderId == 0)
                return RedirectToAction("Login", "Account");

            DateTime today = DateTime.Today;

            IQueryable<DailyReport> query = _db.DailyReports
                .Include(r => r.Recipients)
                    .ThenInclude(rr => rr.Receiver)
                .Where(r => r.SenderId == senderId);

            // ✅ Calendar selected → exact date
            if (selectedDate.HasValue)
            {
                DateTime date = selectedDate.Value.Date;
                query = query.Where(r => r.CreatedDate.Date == date);
            }
            // ✅ Default → last 7 days
            else
            {
                query = query.Where(r => r.CreatedDate.Date >= today.AddDays(-7));
            }

            var reports = query
                .OrderByDescending(r => r.CreatedDate)
                .ToList();

            ViewBag.SelectedDate = selectedDate?.ToString("yyyy-MM-dd");

            return View(reports);
        }


        ///  DeleteSentreport  ////
        //[HttpPost]
        //public IActionResult DeleteSentReport(int reportId)
        //{
        //    int userId = GetCurrentUserId();

        //    var report = _db.DailyReports
        //        .Include(r => r.Recipients)
        //        .FirstOrDefault(r => r.ReportId == reportId && r.SenderId == userId);

        //    if (report == null)
        //        return Json(new { success = false });

        //    _db.DailyReportRecipients.RemoveRange(report.Recipients);
        //    _db.DailyReports.Remove(report);
        //    _db.SaveChanges();

        //    return Json(new { success = true });
        //}

        ///  DeleteAllsentreports  ///
        //[HttpPost]
        //public IActionResult DeleteAllSentReports()
        //{
        //    int userId = GetCurrentUserId();

        //    var reports = _db.DailyReports
        //        .Include(r => r.Recipients)
        //        .Where(r => r.SenderId == userId)
        //        .ToList();

        //    foreach (var r in reports)
        //        _db.DailyReportRecipients.RemoveRange(r.Recipients);

        //    _db.DailyReports.RemoveRange(reports);
        //    _db.SaveChanges();

        //    return Json(new { success = true });
        //}


        // =========================
        // GET: DailyReport/Inbox
        // Default: only TODAY reports
        // range = today | yesterday | last7 | all
        // search = employee name
        // =========================
        public IActionResult Inbox(string? search, DateTime? selectedDate)
        {
            var role = GetCurrentRole();

            // Only Manager / GM / VP / Director allowed (HR denied)
            if (role == "Employee" || role == "HR")
                return RedirectToAction("AccessDenied", "Account");

            int userId = GetCurrentUserId();
            if (userId == 0)
                return RedirectToAction("Login", "Account");

            // ✅ DEFAULT: TODAY
            DateTime filterDate = selectedDate?.Date ?? DateTime.Today;

            var query = _db.DailyReportRecipients
                .Include(r => r.Report)
                    .ThenInclude(rep => rep.Sender)
                .Where(r =>
                    r.ReceiverId == userId &&
                    r.Report != null &&
                    r.Report.CreatedDate.Date == filterDate
                );

            // 🔍 SEARCH BY EMPLOYEE NAME
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(r =>
                    r.Report.Sender != null &&
                    (r.Report.Sender.Name ?? "").ToLower().Contains(search)
                );
            }

            var inbox = query
    .ToList() // ⬅️ execute SQL first
    .OrderByDescending(r => r.Report.CreatedDate)
    .ToList();


            // View data
            ViewBag.Search = search ?? "";
            ViewBag.SelectedDate = filterDate.ToString("yyyy-MM-dd");

            return View(inbox);
        }


        // =========================
        // POST: MarkAsRead
        // =========================
        [HttpPost]
        public IActionResult MarkAsRead(int id)
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return Json(new { success = false });

            var rec = _db.DailyReportRecipients.FirstOrDefault(r => r.Id == id && r.ReceiverId == userId);
            if (rec != null && !rec.IsRead)
            {
                rec.IsRead = true;
                rec.ReadDate = DateTime.Now;
                _db.SaveChanges();
            }

            return Json(new { success = true });
        }

        // =========================
        // DELETE ONE (Inbox item)
        // =========================
        //[HttpPost]
        //public IActionResult DeleteOne(int id, string? search, string range = "today")
        //{
        //    int userId = GetCurrentUserId();
        //    if (userId == 0) return Json(new { success = false });

        //    var rec = _db.DailyReportRecipients.FirstOrDefault(r => r.Id == id && r.ReceiverId == userId);
        //    if (rec != null)
        //    {
        //        _db.DailyReportRecipients.Remove(rec);
        //        _db.SaveChanges();
        //    }

        //    return Json(new { success = true });
        //}

        // =========================
        // DELETE ALL (Inbox filtered)
        // =========================
        //[HttpPost]
        //public IActionResult DeleteAll(string? search, string range = "today")
        //{
        //    int userId = GetCurrentUserId();
        //    if (userId == 0) return Json(new { success = false });

        //    DateTime startDate = DateTime.Today;

        //    switch ((range ?? "today").ToLower())
        //    {
        //        case "yesterday":
        //            startDate = DateTime.Today.AddDays(-1);
        //            break;

        //        case "last7":
        //            startDate = DateTime.Today.AddDays(-7);
        //            break;

        //        case "all":
        //            startDate = DateTime.MinValue;
        //            break;

        //        default:
        //            startDate = DateTime.Today;
        //            break;
        //    }

        //    var query = _db.DailyReportRecipients
        //        .Include(r => r.Report)
        //        .Where(r =>
        //            r.ReceiverId == userId &&
        //            r.Report.CreatedDate >= startDate
        //        );

        //    if (!string.IsNullOrWhiteSpace(search))
        //    {
        //        string s = search.Trim().ToLower();
        //        query = query.Where(r =>
        //            r.Report.Sender.Name.ToLower().Contains(s)
        //        );
        //    }

        //    var list = query.ToList();

        //    _db.DailyReportRecipients.RemoveRange(list);
        //    _db.SaveChanges();

        //    return Json(new { success = true });
        //}

        // =========================
        // Build allowed recipient list based on sender role
        // =========================
        private IEnumerable<SelectListItem> BuildRecipientList(string senderRole)
        {
            senderRole = NormalizeRole(senderRole);

            var allowedRoles = new List<string>();

            switch (senderRole)
            {
                case "Intern":
                    allowedRoles.AddRange(new[] { "Manager", "GM", "VP", "Director" });
                    break;

                case "Employee":
                    allowedRoles.AddRange(new[] { "Manager", "GM", "VP", "Director" });
                    break;

                case "Manager":
                    allowedRoles.AddRange(new[] { "GM", "VP", "Director" });
                    break;

                case "GM":
                    allowedRoles.AddRange(new[] { "VP", "Director" });
                    break;

                case "VP":
                    allowedRoles.Add("Director");
                    break;

                case "HR":
                    allowedRoles.AddRange(new[] { "GM", "VP", "Director" });
                    break;

                case "Director":
                    allowedRoles.Clear();
                    break;
            }

            var allEmployees = _db.Employees.ToList();

            var recipients = allEmployees
                .Where(e => allowedRoles.Contains(NormalizeRole(e.Role)))
                .OrderBy(e => e.Name)
                .Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = $"{e.Name} ({NormalizeRole(e.Role)})"
                })
                .ToList();

            return recipients;
        }
    }
}
