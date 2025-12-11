using HRMS.Data;
using HRMS.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
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

        // =====================================================================
        // EMPLOYEE: GURUKUL MAIN PAGE (WITH PERMISSIONS)
        // =====================================================================
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("Role") != "Employee")
                return RedirectToAction("Login", "Account");

            int? employeeId = HttpContext.Session.GetInt32("EmployeeId");
            if (employeeId == null)
                return RedirectToAction("Login", "Account");

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == employeeId.Value);
            var dept = employee?.Department;

            var videos = await _context.GurukulVideos
                .Where(v =>
                    (v.AllowedDepartment == null && v.AllowedEmployeeId == null) ||
                    (v.AllowedDepartment != null && v.AllowedDepartment == dept && v.AllowedEmployeeId == null) ||
                    (v.AllowedEmployeeId != null && v.AllowedEmployeeId == employeeId)
                )
                .OrderBy(v => v.TitleGroup)
                .ThenBy(v => v.Category)
                .ThenBy(v => v.Title)
                .ToListAsync();

            var groupedData = videos
                .GroupBy(v => string.IsNullOrWhiteSpace(v.TitleGroup) ? "General" : v.TitleGroup.Trim())
                .ToDictionary(
                    tg => tg.Key,
                    tg => tg.GroupBy(v => string.IsNullOrWhiteSpace(v.Category) ? "General" : v.Category.Trim())
                            .ToDictionary(cg => cg.Key, cg => cg.ToList())
                );

            var progress = await _context.GurukulProgress
                .Where(p => p.EmployeeId == employeeId)
                .ToListAsync();

            ViewBag.GroupedData = groupedData;
            ViewBag.Progress = progress;
            ViewBag.EmployeeId = employeeId;

            return View(videos);
        }

        // =====================================================================
        // EMPLOYEE: DETAILS PAGE
        // =====================================================================
        public async Task<IActionResult> Details(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Employee")
                return RedirectToAction("Login", "Account");

            int? employeeId = HttpContext.Session.GetInt32("EmployeeId");
            if (employeeId == null)
                return RedirectToAction("Login", "Account");

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == employeeId.Value);
            var dept = employee?.Department;

            var video = await _context.GurukulVideos.FirstOrDefaultAsync(v => v.Id == id);
            if (video == null)
                return NotFound();

            bool allowed =
                (video.AllowedDepartment == null && video.AllowedEmployeeId == null) ||
                (video.AllowedEmployeeId != null && video.AllowedEmployeeId == employeeId) ||
                (video.AllowedEmployeeId == null && video.AllowedDepartment != null && video.AllowedDepartment == dept);

            if (!allowed)
                return Forbid();

            string titleGroup = string.IsNullOrWhiteSpace(video.TitleGroup) ? "General" : video.TitleGroup.Trim();
            string category = string.IsNullOrWhiteSpace(video.Category) ? "General" : video.Category.Trim();

            var relatedVideos = await _context.GurukulVideos
                .Where(v =>
                    (string.IsNullOrWhiteSpace(v.TitleGroup) ? "General" : v.TitleGroup.Trim()) == titleGroup &&
                    (string.IsNullOrWhiteSpace(v.Category) ? "General" : v.Category.Trim()) == category &&
                    (
                        (v.AllowedDepartment == null && v.AllowedEmployeeId == null) ||
                        (v.AllowedEmployeeId != null && v.AllowedEmployeeId == employeeId) ||
                        (v.AllowedEmployeeId == null && v.AllowedDepartment != null && v.AllowedDepartment == dept)
                    )
                )
                .ToListAsync();

            // ⭐ FIX: Always sort EXACTLY like Index page so NEXT button works correctly
            relatedVideos = relatedVideos
                .OrderBy(v => v.TitleGroup)
                .ThenBy(v => v.Category)
                .ThenBy(v => v.Title)
                .ToList();

            ViewBag.VideoList = relatedVideos;

            ViewBag.Progress = await _context.GurukulProgress
                .FirstOrDefaultAsync(p => p.EmployeeId == employeeId && p.VideoId == id);

            ViewBag.EmployeeId = employeeId;

            return View(video);
        }


        // =====================================================================
        // EMPLOYEE: MARK COMPLETE
        // =====================================================================
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

        // =====================================================================
        // HR LIST
        // =====================================================================
        public async Task<IActionResult> HRList()
        {
            if (HttpContext.Session.GetString("Role") != "HR")
                return RedirectToAction("Login", "Account");

            var videos = await _context.GurukulVideos
                .Include(v => v.AllowedEmployee)
                .OrderByDescending(v => v.UploadedOn)
                .ToListAsync();

            return View(videos);
        }

        // =====================================================================
        // HR CREATE (GET)
        // =====================================================================
        public async Task<IActionResult> Create()
        {
            // ❌ Removed HR role check
            await PopulatePermissionDropdowns();
            return View();
        }

        // =====================================================================
        // HR CREATE (POST)
        // =====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GurukulVideo model, IFormFile? VideoFile, string? ExternalLink)
        {
            // ❌ Removed HR role check

            model.Category = string.IsNullOrWhiteSpace(model.Category) ? "General" : model.Category.Trim();
            model.TitleGroup = string.IsNullOrWhiteSpace(model.TitleGroup) ? "General" : model.TitleGroup.Trim();
            model.Title = string.IsNullOrWhiteSpace(model.Title) ? "Untitled" : model.Title.Trim();

            if (!ModelState.IsValid)
            {
                await PopulatePermissionDropdowns();
                return View(model);
            }

            // ================================
            // ⭐ CUSTOM SERVER STORAGE PATH
            // ================================
            string videoFolder = @"C:\HRMSFiles\Gurukul Videos";
            string thumbFolder = @"C:\HRMSFiles\Gurukul Videos\Thumbs";
            string ffmpegPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ffmpeg", "ffmpeg.exe");

            Directory.CreateDirectory(videoFolder);
            Directory.CreateDirectory(thumbFolder);

            // ------------------------
            // SAVE VIDEO OR LINK
            // ------------------------
            if (VideoFile != null && VideoFile.Length > 0)
            {
                string fileName = Guid.NewGuid() + Path.GetExtension(VideoFile.FileName);
                string fullVideoPath = Path.Combine(videoFolder, fileName);

                using (var fs = new FileStream(fullVideoPath, FileMode.Create))
                {
                    await VideoFile.CopyToAsync(fs);
                }

                // Store server path in DB OR a relative path — your choice:
                model.VideoPath = fullVideoPath;   // ← FULL PATH stored
                model.IsExternal = false;

                // Create thumbnail
                string thumbFileName = Path.GetFileNameWithoutExtension(fileName) + ".jpg";
                string fullThumbPath = Path.Combine(thumbFolder, thumbFileName);

                var process = new Process();
                process.StartInfo.FileName = ffmpegPath;
                process.StartInfo.Arguments = $"-i \"{fullVideoPath}\" -ss 00:00:01 -vframes 1 \"{fullThumbPath}\"";
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;

                process.Start();
                process.WaitForExit();

                model.ThumbnailPath = fullThumbPath;   // Saving full server path
            }
            else if (!string.IsNullOrWhiteSpace(ExternalLink))
            {
                string link = ExternalLink.Trim();
                model.VideoPath = link;
                model.IsExternal = true;
                model.ThumbnailPath = GetYouTubeThumbnail(link);
            }
            else
            {
                await PopulatePermissionDropdowns();
                ModelState.AddModelError("", "Upload a file OR enter a link.");
                return View(model);
            }

            model.UploadedOn = DateTime.UtcNow;

            _context.GurukulVideos.Add(model);
            await _context.SaveChangesAsync();

            // ================================
            // ⭐ AUTOMATIC ANNOUNCEMENT ⭐
            // ================================
            var announcement = new Announcement
            {
                Title = "New Gurukul Video Added",
                Message = $"{model.Title} is now available to view.",
                CreatedOn = DateTime.UtcNow,
                ReadByEmployees = ""
            };

            if (model.AllowedDepartment == null && model.AllowedEmployeeId == null)
            {
                announcement.IsGlobal = true;
            }
            else if (model.AllowedEmployeeId != null)
            {
                announcement.IsGlobal = false;
                announcement.TargetEmployees = model.AllowedEmployeeId.ToString();
            }
            else if (model.AllowedDepartment != null)
            {
                announcement.IsGlobal = false;
                announcement.TargetDepartments = model.AllowedDepartment;
            }

            _context.Announcements.Add(announcement);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(HRList));
        }


        // =====================================================================
        // PERMISSION DROPDOWN HELPER
        // =====================================================================
        private async Task PopulatePermissionDropdowns()
        {
            var departmentItems = await _context.Employees
                .Where(e => !string.IsNullOrEmpty(e.Department))
                .Select(e => e.Department)
                .Distinct()
                .OrderBy(d => d)
                .Select(d => new SelectListItem { Value = d, Text = d })
                .ToListAsync();

            departmentItems.Insert(0, new SelectListItem { Value = "", Text = "All departments" });

            var employeeItems = await _context.Employees
                .OrderBy(e => e.Name)
                .Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = $"{e.Name} ({e.EmployeeCode})"
                })
                .ToListAsync();

            employeeItems.Insert(0, new SelectListItem { Value = "", Text = "All employees" });

            ViewBag.DepartmentItems = departmentItems;
            ViewBag.EmployeeItems = employeeItems;
        }

        // =====================================================================
        // HR PROGRESS PAGE (WITH STATUS + SEARCH + DEPARTMENT FILTER)
        // =====================================================================
        public async Task<IActionResult> Progress(int id, string? status, string? search, string? department)
        {
            if (HttpContext.Session.GetString("Role") != "HR")
                return RedirectToAction("Login", "Account");

            var video = await _context.GurukulVideos.FindAsync(id);
            if (video == null) return NotFound();

            // ==========================================
            // 1️⃣ LOAD EMPLOYEES BASED ON VIDEO PERMISSION
            // ==========================================
            List<Employee> allEmployees;

            if (!string.IsNullOrEmpty(video.AllowedDepartment))
            {
                // Video limited to a specific department
                allEmployees = await _context.Employees
                    .Where(e => e.Department == video.AllowedDepartment)
                    .OrderBy(e => e.Name)
                    .ToListAsync();
            }
            else if (video.AllowedEmployeeId != null)
            {
                // Video limited to one employee
                allEmployees = await _context.Employees
                    .Where(e => e.Id == video.AllowedEmployeeId)
                    .ToListAsync();
            }
            else
            {
                // Video visible to all employees
                allEmployees = await _context.Employees
                    .OrderBy(e => e.Name)
                    .ToListAsync();
            }

            // ==========================================
            // 2️⃣ CREATE DEPARTMENT DROPDOWN (ONLY VALID ONES)
            // ==========================================
            var departmentList = allEmployees
                .Where(e => !string.IsNullOrEmpty(e.Department))
                .Select(e => e.Department)
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            ViewBag.DepartmentList = departmentList;
            ViewBag.SelectedDepartment = department;

            // ==========================================
            // 3️⃣ LOAD PROGRESS FOR THIS VIDEO
            // ==========================================
            var progress = await _context.GurukulProgress
                .Include(p => p.Employee)
                .Where(p => p.VideoId == id)
                .ToListAsync();

            // Employees who STILL have no record = pending
            var pending = allEmployees
                .Where(e => !progress.Any(p => p.EmployeeId == e.Id))
                .Select(e => new GurukulProgress
                {
                    EmployeeId = e.Id,
                    Employee = e,
                    VideoId = id,
                    IsCompleted = false,
                    CompletedOn = null
                });

            var finalList = progress.Concat(pending)
                                    .OrderBy(p => p.Employee.Name)
                                    .ToList();

            // ==========================================
            // 4️⃣ FILTER — DEPARTMENT
            // ==========================================
            if (!string.IsNullOrWhiteSpace(department) && department != "ALL")
            {
                finalList = finalList
                    .Where(p => p.Employee.Department == department)
                    .ToList();
            }

            // ==========================================
            // 5️⃣ FILTER — STATUS
            // ==========================================
            if (status == "completed")
                finalList = finalList.Where(p => p.IsCompleted).ToList();
            else if (status == "pending")
                finalList = finalList.Where(p => !p.IsCompleted).ToList();

            // ==========================================
            // 6️⃣ FILTER — EMPLOYEE NAME SEARCH
            // ==========================================
            if (!string.IsNullOrWhiteSpace(search))
            {
                string term = search.Trim().ToLower();
                finalList = finalList
                    .Where(p => p.Employee.Name.ToLower().Contains(term))
                    .ToList();
            }

            // ==========================================
            // 7️⃣ SEND DATA TO VIEW
            // ==========================================
            ViewBag.Video = video;
            ViewBag.SelectedStatus = status;
            ViewBag.Search = search;

            return View(finalList);
        }


        // =====================================================================
        // GET YOUTUBE THUMBNAIL
        // =====================================================================
        private string? GetYouTubeThumbnail(string url)
        {
            try
            {
                string? videoId = null;
                var uri = new Uri(url);

                if (uri.Host.Contains("youtu.be"))
                    videoId = uri.AbsolutePath.Trim('/');
                else if (uri.Host.Contains("youtube.com"))
                {
                    var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
                    if (query.TryGetValue("v", out var v))
                        videoId = v.ToString();
                }

                return videoId != null ? $"https://img.youtube.com/vi/{videoId}/hqdefault.jpg" : null;
            }
            catch { return null; }
        }

        // =====================================================================
        // DELETE VIDEO
        // =====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (HttpContext.Session.GetString("Role") != "HR")
                return RedirectToAction("Login", "Account");

            var video = await _context.GurukulVideos.FindAsync(id);
            if (video != null)
            {
                if (!video.IsExternal)
                {
                    string full = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        video.VideoPath.TrimStart('/')
                            .Replace('/', Path.DirectorySeparatorChar)
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