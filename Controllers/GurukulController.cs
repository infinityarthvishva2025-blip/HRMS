using HRMS.Data;
using HRMS.Models;
using HRMS.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Controllers
{
    public class GurukulController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GurukulController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==================== HR SECTION ====================

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Role") != "HR")
                return RedirectToAction("Login", "Account");

            var videos = _context.GurukulVideos.OrderByDescending(v => v.UploadedOn).ToList();
            return View(videos);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("Role") != "HR")
                return RedirectToAction("Login", "Account");

            return View();
        }

        [HttpPost]
        public IActionResult Create(GurukulVideo model)
        {
            if (!ModelState.IsValid)
                return View(model);

            _context.GurukulVideos.Add(model);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var video = _context.GurukulVideos.Find(id);
            if (video != null)
            {
                _context.GurukulVideos.Remove(video);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public IActionResult ProgressReport()
        {
            // ✅ Only HR can access
            if (HttpContext.Session.GetString("Role") != "HR")
                return RedirectToAction("Login", "Account");

            // ✅ Join employees, videos, and progress
            var report = from emp in _context.Employees
                         from vid in _context.GurukulVideos
                         join prog in _context.GurukulProgress
                         on new { E = emp.Id, V = vid.Id } equals new { E = prog.EmployeeId, V = prog.VideoId } into gj
                         from sub in gj.DefaultIfEmpty()
                         select new
                         {
                             emp.Name,
                             emp.EmployeeCode,
                             vid.Title,
                             vid.Category,
                             IsCompleted = sub != null && sub.IsCompleted,
                             CompletedOn = sub != null ? sub.CompletedOn : null
                         };

            // ✅ Group by employee and build ViewModel
            var grouped = report
                .GroupBy(r => new { r.Name, r.EmployeeCode })
                .Select(g => new GurukulProgressReportViewModel
                {
                    EmployeeName = g.Key.Name,
                    EmployeeCode = g.Key.EmployeeCode,
                    TotalVideos = g.Count(),
                    CompletedVideos = g.Count(x => x.IsCompleted),
                    ProgressPercentage = g.Count() > 0
                        ? (int)((double)g.Count(x => x.IsCompleted) / g.Count() * 100)
                        : 0,
                    Details = g.Select(x => new VideoProgressDetail
                    {
                        Title = x.Title,
                        Category = x.Category,
                        IsCompleted = x.IsCompleted,
                        CompletedOn = x.CompletedOn
                    }).ToList()
                })
                .OrderBy(x => x.EmployeeName)
                .ToList();

            // ✅ Send to View
            return View(grouped);
        }

        // ==================== EMPLOYEE SECTION ====================

        public IActionResult EmployeeGurukul(string category)
        {
            if (HttpContext.Session.GetString("Role") != "Employee")
                return RedirectToAction("Login", "Account");

            int? empId = HttpContext.Session.GetInt32("EmployeeId");

            var videos = _context.GurukulVideos.AsQueryable();
            if (!string.IsNullOrEmpty(category))
                videos = videos.Where(v => v.Category == category);

            var allVideos = _context.GurukulVideos.ToList();
            var progress = _context.GurukulProgress.Where(p => p.EmployeeId == empId).ToList();

            var model = videos
                .OrderBy(v => v.Category)
                .ToList()
                .Select(v => new
                {
                    v.Id,
                    v.Title,
                    v.Category,
                    v.Description,
                    v.VideoUrl,
                    IsCompleted = progress.Any(p => p.VideoId == v.Id && p.IsCompleted)
                }).ToList();

            ViewBag.Categories = allVideos.Select(v => v.Category).Distinct().ToList();

            int total = allVideos.Count;
            int completed = progress.Count(p => p.IsCompleted);
            ViewBag.ProgressPercent = total > 0 ? (completed * 100 / total) : 0;

            return View(model);
        }

        [HttpPost]
        public IActionResult MarkCompleted(int videoId)
        {
            int? empId = HttpContext.Session.GetInt32("EmployeeId");
            if (empId == null)
                return Json(new { success = false, message = "Not logged in" });

            var existing = _context.GurukulProgress.FirstOrDefault(p => p.EmployeeId == empId && p.VideoId == videoId);

            if (existing == null)
            {
                _context.GurukulProgress.Add(new GurukulProgress
                {
                    EmployeeId = empId.Value,
                    VideoId = videoId,
                    IsCompleted = true,
                    CompletedOn = DateTime.Now
                });
            }
            else
            {
                existing.IsCompleted = true;
                existing.CompletedOn = DateTime.Now;
            }

            _context.SaveChanges();
            return Json(new { success = true });
        }
    }
}
