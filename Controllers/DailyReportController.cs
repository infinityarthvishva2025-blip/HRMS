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

        public DailyReportController(
     ApplicationDbContext db,
     IWebHostEnvironment env
 )
        {
            _db = db;
            _env = env;
        }


        // =========================
        // SESSION HELPERS (FIXED)
        // =========================
        private int GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("EmployeeId") ?? 0;
        }

        private string GetCurrentRole()
        {
            return HttpContext.Session.GetString("Role") ?? "Employee";
        }



        private string NormalizeRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role))
                return "Employee";

            role = role.Trim();

            if (role.Equals("admin", StringComparison.OrdinalIgnoreCase))
                return "HR";

            return role switch
            {
                "Manager" => "Manager",
                "GM" => "GM",
                "VP" => "VP",
                "Director" => "Director",
                _ => "Employee"
            };
        }

        // =========================
        // GET: DailyReport/Send
        // =========================
        public IActionResult Send()
        {
            string role = GetCurrentRole();

            // Directors cannot send reports
            if (role == "Director")
            {
                return RedirectToAction("AccessDenied", "Account");
            }

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
            string role = NormalizeRole(GetCurrentRole());

            // ❌ Director cannot submit
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

            // =========================
            // VALIDATE RECIPIENTS
            // =========================
            if (model.SelectedRecipientIds == null || !model.SelectedRecipientIds.Any())
            {
                ModelState.AddModelError("SelectedRecipientIds",
                    "Please select at least one recipient.");
            }

            if (!ModelState.IsValid)
            {
                model.RecipientList = BuildRecipientList(role);
                return View(model);
            }

            // =========================
            // FILE UPLOAD (OPTIONAL)
            // =========================
            string fileName = null; // ✅ ONLY filename

            if (model.Attachment != null && model.Attachment.Length > 0)
            {
                var ext = Path.GetExtension(model.Attachment.FileName).ToLower();

                if (!new[] { ".jpg", ".jpeg", ".png", ".pdf" }.Contains(ext))
                {
                    ModelState.AddModelError("Attachment",
                        "Only JPG, PNG or PDF files are allowed.");
                    model.RecipientList = BuildRecipientList(role);
                    return View(model);
                }

                string uploadsFolder = Path.Combine(
                    _env.WebRootPath,
                    "uploads",
                    "dailyreports"
                );

                Directory.CreateDirectory(uploadsFolder);

                fileName = $"{Guid.NewGuid()}{ext}";
                string fullPath = Path.Combine(uploadsFolder, fileName);

                using var stream = new FileStream(fullPath, FileMode.Create);
                model.Attachment.CopyTo(stream);
            }

            // =========================
            // SAVE DAILY REPORT
            // =========================
            var report = new DailyReport
            {
                SenderId = senderId,
                TodaysWork = model.TodaysWork,
                PendingWork = model.PendingWork,
                Issues = model.Issues,
                AttachmentPath = fileName, // ✅ ONLY filename
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
                    IsRead = false
                });
            }

            _db.SaveChanges();

            // ===============================
            // AUTO CHECKOUT AFTER DAILY REPORT
            // ===============================
            var empCode = sender.EmployeeCode;

            if (!string.IsNullOrEmpty(empCode))
            {
                var attendance = _db.Attendances.FirstOrDefault(a =>
                    a.Emp_Code == empCode &&
                    a.Date == DateTime.Today &&
                    a.OutTime == null
                );

                if (attendance != null && attendance.InTime != null)
                {
                    attendance.OutTime = DateTime.Now.TimeOfDay;

                    TimeSpan worked =
                        attendance.OutTime.Value - attendance.InTime.Value;

                    attendance.Total_Hours =
                        Math.Round((decimal)worked.TotalHours, 2);

                    attendance.Status = "Present";

                    _db.SaveChanges();
                }
            }

            return RedirectToAction("MySentReports");
        }


        // =========================
        // GET: DailyReport/Success
        // =========================

        public IActionResult Success()
        {
            var role = GetCurrentRole();

            // Employee & Manager should NEVER go to HR dashboard
            if (role == "Employee" || role == "Manager")
            {
                return RedirectToAction("MySentReports", "DailyReport");
                // OR EmployeePanel if you prefer
                // return RedirectToAction("EmployeePanel", "Attendance");
            }

            // GM, VP, Director, HR → HR dashboard
            return RedirectToAction("Index", "Home");
        }



        // GET: DailyReport/MySentReports
        public IActionResult MySentReports()
        {
            int senderId = GetCurrentUserId();
            if (senderId == 0)
                return RedirectToAction("Login", "Account");

            var reports = _db.DailyReports
                .Where(r => r.SenderId == senderId)
                .Include(r => r.Recipients)
                    .ThenInclude(rr => rr.Receiver)   // 🔥 THIS LINE WAS MISSING
                .OrderByDescending(r => r.CreatedDate)
                .ToList();

            return View(reports);
        }




        // GET: DailyReport/Inbox
        public IActionResult Inbox()
        {
            var role = GetCurrentRole();

            // Employees & HR cannot access inbox
            if (role == "Employee" || role == "HR")
                return RedirectToAction("AccessDenied", "Account");

            int userId = GetCurrentUserId();

            var inbox = _db.DailyReportRecipients
                .Include(r => r.Report)
                    .ThenInclude(r => r.Sender)
                .Where(r => r.ReceiverId == userId)
                .OrderByDescending(r => r.Report.CreatedDate)
                .ToList();

            return View(inbox);
        }



        [HttpPost]
        public IActionResult MarkAsRead(int id)
        {
            var rec = _db.DailyReportRecipients.FirstOrDefault(r => r.Id == id);
            if (rec != null && !rec.IsRead)
            {
                rec.IsRead = true;
                rec.ReadDate = DateTime.Now;
                _db.SaveChanges();
            }

            return Json(new { success = true });
        }

        // Build allowed recipient list based on sender role
        private IEnumerable<SelectListItem> BuildRecipientList(string senderRole)
        {
            var allowedRoles = new List<string>();

            switch (senderRole)
            {
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
                    // Director should not send anyone
                    allowedRoles.Clear();
                    break;
            }

            // load employees and filter in memory (because NormalizeRole is C# method)
            var allEmployees = _db.Employees.ToList();

            var recipients = allEmployees
                .Where(e => allowedRoles.Contains(NormalizeRole(e.Role)))
                .OrderBy(e => e.Name)   // adjust Name property if different
                .ToList();

            return recipients.Select(e => new SelectListItem
            {
                Value = e.Id.ToString(),
                Text = $"{e.Name} ({NormalizeRole(e.Role)})"
            });
        }
    }
}
