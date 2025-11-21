using HRMS.Data;
using HRMS.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HRMS.Controllers
{
    public class GurukulController : Controller
    {
        private readonly ApplicationDbContext _context;
        public GurukulController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // EMPLOYEE: GURUKUL MAIN PAGE (TitleGroup → Category → Videos)
        // ============================================================
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("Role") != "Employee")
                return RedirectToAction("Login", "Account");

            int? employeeId = HttpContext.Session.GetInt32("EmployeeId");
            if (employeeId == null)
                return RedirectToAction("Login", "Account");

            var videos = await _context.GurukulVideos
                .OrderBy(v => v.TitleGroup)
                .ThenBy(v => v.Category)
                .ThenBy(v => v.Title)
                .ToListAsync();

            // FIXED GROUPING: TitleGroup → Category → Videos
            var groupedData = videos
                .GroupBy(v => (string.IsNullOrWhiteSpace(v.TitleGroup) ? "General" : v.TitleGroup.Trim()))
                .ToDictionary(
                    tg => tg.Key,
                    tg => tg.GroupBy(v => (string.IsNullOrWhiteSpace(v.Category) ? "General" : v.Category.Trim()))
                            .ToDictionary(cat => cat.Key, cat => cat.ToList())
                );

            // Employee progress
            var progress = await _context.GurukulProgress
                .Where(p => p.EmployeeId == employeeId)
                .ToListAsync();

            ViewBag.GroupedData = groupedData;
            ViewBag.Progress = progress;
            ViewBag.EmployeeId = employeeId;

            return View(videos);
        }

        // ============================================================
        // EMPLOYEE: DETAILS PAGE
        // ============================================================
        public async Task<IActionResult> Details(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Employee")
                return RedirectToAction("Login", "Account");

            int? employeeId = HttpContext.Session.GetInt32("EmployeeId");
            if (employeeId == null)
                return RedirectToAction("Login", "Account");

            var video = await _context.GurukulVideos
                .FirstOrDefaultAsync(v => v.Id == id);

            if (video == null)
                return NotFound();

            string titleGroup = (string.IsNullOrWhiteSpace(video.TitleGroup) ? "General" : video.TitleGroup.Trim());
            string category = (string.IsNullOrWhiteSpace(video.Category) ? "General" : video.Category.Trim());

            // Load SAME TitleGroup + Category
            var list = await _context.GurukulVideos
                .Where(v =>
                    (string.IsNullOrWhiteSpace(v.TitleGroup) ? "General" : v.TitleGroup.Trim()) == titleGroup &&
                    (string.IsNullOrWhiteSpace(v.Category) ? "General" : v.Category.Trim()) == category
                )
                .OrderBy(v => v.Title)
                .ToListAsync();

            ViewBag.VideoList = list;

            var prog = await _context.GurukulProgress
                .FirstOrDefaultAsync(p => p.EmployeeId == employeeId && p.VideoId == id);

            ViewBag.Progress = prog;
            ViewBag.EmployeeId = employeeId;

            return View(video);
        }

        // ============================================================
        // EMPLOYEE: MARK COMPLETE
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> MarkComplete(int videoId)
        {
            if (HttpContext.Session.GetString("Role") != "Employee")
                return Unauthorized();

            int? employeeId = HttpContext.Session.GetInt32("EmployeeId");
            if (employeeId == null)
                return Unauthorized();

            var record = await _context.GurukulProgress
                .FirstOrDefaultAsync(p => p.VideoId == videoId && p.EmployeeId == employeeId.Value);

            if (record == null)
            {
                record = new GurukulProgress
                {
                    VideoId = videoId,
                    EmployeeId = employeeId.Value,
                    IsCompleted = true,
                    CompletedOn = DateTime.UtcNow
                };
                _context.GurukulProgress.Add(record);
            }
            else
            {
                record.IsCompleted = true;
                record.CompletedOn = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        // ============================================================
        // HR PANEL
        // ============================================================

        public async Task<IActionResult> HRList()
        {
            if (HttpContext.Session.GetString("Role") != "HR")
                return RedirectToAction("Login", "Account");

            var videos = await _context.GurukulVideos
                .OrderByDescending(v => v.UploadedOn)
                .ToListAsync();

            return View(videos);
        }

        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("Role") != "HR")
                return RedirectToAction("Login", "Account");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GurukulVideo model, IFormFile? VideoFile, string? ExternalLink)
        {
            if (HttpContext.Session.GetString("Role") != "HR")
                return RedirectToAction("Login", "Account");

            model.Category = string.IsNullOrWhiteSpace(model.Category) ? "General" : model.Category.Trim();
            model.TitleGroup = string.IsNullOrWhiteSpace(model.TitleGroup) ? "General" : model.TitleGroup.Trim();
            model.Title = string.IsNullOrWhiteSpace(model.Title) ? "Untitled" : model.Title.Trim();

            if (!ModelState.IsValid)
                return View(model);

            // FILE or LINK
            if (VideoFile != null && VideoFile.Length > 0)
            {
                string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "gurukul");
                Directory.CreateDirectory(folder);

                string fileName = Guid.NewGuid() + Path.GetExtension(VideoFile.FileName);
                string fullPath = Path.Combine(folder, fileName);

                using var fs = new FileStream(fullPath, FileMode.Create);
                await VideoFile.CopyToAsync(fs);

                model.VideoPath = "/uploads/gurukul/" + fileName;
                model.IsExternal = false;
            }
            else if (!string.IsNullOrWhiteSpace(ExternalLink))
            {
                model.VideoPath = ExternalLink.Trim();
                model.IsExternal = true;
            }
            else
            {
                ModelState.AddModelError("", "Upload a file OR enter an external link.");
                return View(model);
            }

            model.UploadedOn = DateTime.UtcNow;

            _context.GurukulVideos.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(HRList));
        }

        public async Task<IActionResult> Progress(int id)
        {
            if (HttpContext.Session.GetString("Role") != "HR")
                return RedirectToAction("Login", "Account");

            var video = await _context.GurukulVideos.FindAsync(id);
            if (video == null) return NotFound();

            var allEmployees = await _context.Employees.OrderBy(e => e.Name).ToListAsync();

            var progress = await _context.GurukulProgress
                .Include(p => p.Employee)
                .Where(p => p.VideoId == id)
                .ToListAsync();

            // FIND employees who did NOT start
            var pendingEmployees = allEmployees
                .Where(e => !progress.Any(p => p.EmployeeId == e.Id))
                .Select(e => new GurukulProgress
                {
                    EmployeeId = e.Id,
                    Employee = e,
                    VideoId = id,
                    IsCompleted = false,
                    CompletedOn = null
                })
                .ToList();

            // COMBINE
            var finalList = progress.Concat(pendingEmployees)
                .OrderBy(p => p.Employee.Name)
                .ToList();

            ViewBag.Video = video;
            return View(finalList);
        }

        public async Task<IActionResult> ProgressAll(int? employeeId, string? status)
        {
            if (HttpContext.Session.GetString("Role") != "HR")
                return RedirectToAction("Login", "Account");

            var employees = await _context.Employees
                .OrderBy(e => e.Name)
                .ToListAsync();

            var videos = await _context.GurukulVideos
                .OrderBy(v => v.TitleGroup)
                .ThenBy(v => v.Category)
                .ThenBy(v => v.Title)
                .ToListAsync();

            var progress = await _context.GurukulProgress
                .Include(p => p.Employee)
                .Include(p => p.Video)
                .ToListAsync();

            // FILTERING
            if (employeeId.HasValue)
                progress = progress.Where(p => p.EmployeeId == employeeId.Value).ToList();

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status == "completed")
                    progress = progress.Where(p => p.IsCompleted).ToList();
                else if (status == "pending")
                    progress = progress.Where(p => !p.IsCompleted).ToList();
            }

            ViewBag.Employees = employees;
            ViewBag.Videos = videos;
            ViewBag.SelectedEmployee = employeeId;
            ViewBag.SelectedStatus = status;

            return View(progress);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (HttpContext.Session.GetString("Role") != "HR")
                return RedirectToAction("Login", "Account");

            var video = await _context.GurukulVideos.FindAsync(id);
            if (video != null)
            {
                if (!video.IsExternal && !string.IsNullOrEmpty(video.VideoPath))
                {
                    var full = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        video.VideoPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)
                    );

                    if (System.IO.File.Exists(full))
                        System.IO.File.Delete(full);
                }

                _context.GurukulVideos.Remove(video);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(HRList));
        }
    }
}
