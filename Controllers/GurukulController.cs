using HRMS.Data;
using HRMS.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        // ============================================================
        // EMPLOYEE: GURUKUL MAIN PAGE
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

            var groupedData = videos
                .GroupBy(v => string.IsNullOrWhiteSpace(v.TitleGroup) ? "General" : v.TitleGroup.Trim())
                .ToDictionary(
                    tg => tg.Key,
                    tg => tg.GroupBy(v => string.IsNullOrWhiteSpace(v.Category) ? "General" : v.Category.Trim())
                            .ToDictionary(cat => cat.Key, cat => cat.ToList())
                );

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

            var video = await _context.GurukulVideos.FirstOrDefaultAsync(v => v.Id == id);

            if (video == null)
                return NotFound();

            string titleGroup = string.IsNullOrWhiteSpace(video.TitleGroup) ? "General" : video.TitleGroup.Trim();
            string category = string.IsNullOrWhiteSpace(video.Category) ? "General" : video.Category.Trim();

            var list = await _context.GurukulVideos
                .Where(v =>
                    (string.IsNullOrWhiteSpace(v.TitleGroup) ? "General" : v.TitleGroup.Trim()) == titleGroup &&
                    (string.IsNullOrWhiteSpace(v.Category) ? "General" : v.Category.Trim()) == category
                )
                .OrderBy(v => v.Title)
                .ToListAsync();

            ViewBag.VideoList = list;
            ViewBag.Progress = await _context.GurukulProgress
                .FirstOrDefaultAsync(p => p.EmployeeId == employeeId && p.VideoId == id);

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

            return View(await _context.GurukulVideos
                .OrderByDescending(v => v.UploadedOn)
                .ToListAsync());
        }

        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("Role") != "HR")
                return RedirectToAction("Login", "Account");

            return View();
        }

        // ============================================================
        // HR: CREATE VIDEO WITH THUMBNAIL (RAW FFMPEG)
        // ============================================================
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

            string root = Directory.GetCurrentDirectory();
            string videoFolder = Path.Combine(root, "wwwroot", "uploads", "gurukul");
            string thumbFolder = Path.Combine(root, "wwwroot", "uploads", "gurukul-thumbs");
            string ffmpegPath = Path.Combine(root, "wwwroot", "ffmpeg", "ffmpeg.exe");

            Directory.CreateDirectory(videoFolder);
            Directory.CreateDirectory(thumbFolder);

            // CASE 1 — INTERNAL MP4
            if (VideoFile != null && VideoFile.Length > 0)
            {
                string fileName = Guid.NewGuid() + Path.GetExtension(VideoFile.FileName);
                string fullVideoPath = Path.Combine(videoFolder, fileName);

                using (var fs = new FileStream(fullVideoPath, FileMode.Create))
                {
                    await VideoFile.CopyToAsync(fs);
                }

                model.VideoPath = "/uploads/gurukul/" + fileName;
                model.IsExternal = false;

                // THUMBNAIL NAME
                string thumbFileName = Path.GetFileNameWithoutExtension(fileName) + ".jpg";
                string fullThumbPath = Path.Combine(thumbFolder, thumbFileName);

                // RAW FFMPEG COMMAND
                var process = new Process();
                process.StartInfo.FileName = ffmpegPath;
                process.StartInfo.Arguments = $"-i \"{fullVideoPath}\" -ss 00:00:01 -vframes 1 \"{fullThumbPath}\"";
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;

                process.Start();
                process.WaitForExit();

                model.ThumbnailPath = "/uploads/gurukul-thumbs/" + thumbFileName;
            }
            // CASE 2 — YOUTUBE LINK
            else if (!string.IsNullOrWhiteSpace(ExternalLink))
            {
                string link = ExternalLink.Trim();
                model.VideoPath = link;
                model.IsExternal = true;
                model.ThumbnailPath = GetYouTubeThumbnail(link);
            }
            else
            {
                ModelState.AddModelError("", "Upload a file OR enter a link.");
                return View(model);
            }

            model.UploadedOn = DateTime.UtcNow;

            _context.GurukulVideos.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(HRList));
        }


        // ============================================================
        // HR: VIDEO PROGRESS PAGE (WITH FILTERS)
        // ============================================================
        public async Task<IActionResult> Progress(int id, string? status, string? search)
        {
            if (HttpContext.Session.GetString("Role") != "HR")
                return RedirectToAction("Login", "Account");

            var video = await _context.GurukulVideos.FindAsync(id);
            if (video == null) return NotFound();

            // All employees
            var allEmployees = await _context.Employees
                .OrderBy(e => e.Name)
                .ToListAsync();

            // Actual progress records
            var progress = await _context.GurukulProgress
                .Include(p => p.Employee)
                .Where(p => p.VideoId == id)
                .ToListAsync();

            // Employees who have no progress entry yet (Pending by default)
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

            var finalList = progress.Concat(pendingEmployees)
                .OrderBy(p => p.Employee.Name)
                .ToList();

            // ===== FILTER: Completed / Pending =====
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status == "completed")
                    finalList = finalList.Where(p => p.IsCompleted).ToList();
                else if (status == "pending")
                    finalList = finalList.Where(p => !p.IsCompleted).ToList();
            }

            // ===== SEARCH: Employee Name =====
            if (!string.IsNullOrWhiteSpace(search))
            {
                string term = search.Trim().ToLower();
                finalList = finalList
                    .Where(p => p.Employee.Name.ToLower().Contains(term))
                    .ToList();
            }

            ViewBag.Video = video;
            ViewBag.SelectedStatus = status;
            ViewBag.Search = search;

            return View(finalList);
        }
        // ============================================================
        // Get YouTube Thumbnail
        // ============================================================
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

        // ============================================================
        // HR: DELETE VIDEO
        // ============================================================
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
