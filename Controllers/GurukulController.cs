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

        // GET: Create Page
        [HttpGet]
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("Role") != "HR")
                return RedirectToAction("Login", "Account");

            return View();
        }

        // POST: Upload Video
        [HttpPost]
        public async Task<IActionResult> Create(GurukulVideo model, IFormFile VideoFile)
        {
            if (VideoFile == null || VideoFile.Length == 0)
            {
                ModelState.AddModelError("", "Please upload a video file.");
                return View(model);
            }

            // Upload video
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/videos");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(VideoFile.FileName);
            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await VideoFile.CopyToAsync(stream);
            }

            model.VideoPath = "/videos/" + fileName;

            _context.GurukulVideos.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }


        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Role") != "HR")
                return RedirectToAction("Login", "Account");

            var videos = _context.GurukulVideos.OrderByDescending(v => v.UploadedOn).ToList();
            return View(videos);
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

        // ==================== EMPLOYEE & PROGRESS ====================

        public IActionResult ProgressReport()
        {
            if (HttpContext.Session.GetString("Role") != "HR")
                return RedirectToAction("Login", "Account");

            var report = from emp in _context.Employees
                         from vid in _context.GurukulVideos
                         join prog in _context.GurukulProgress
                         on new { E = emp.Id, V = vid.Id }
                         equals new { E = prog.EmployeeId, V = prog.VideoId }
                         into gj
                         from sub in gj.DefaultIfEmpty()
                         select new
                         {
                             EmployeeId = emp.Id,
                             EmployeeName = emp.Name,
                             EmployeeCode = emp.EmployeeCode,

                             VideoId = vid.Id,
                             VideoTitle = vid.Title,
                             VideoCategory = vid.Category,

                             IsCompleted = sub != null && sub.IsCompleted,
                             CompletedOn = sub != null ? sub.CompletedOn : null
                         };

            var grouped = report
                .GroupBy(r => new { r.EmployeeName, r.EmployeeCode })
                .Select(g => new GurukulProgressReportViewModel
                {
                    EmployeeName = g.Key.EmployeeName,
                    EmployeeCode = g.Key.EmployeeCode,
                    TotalVideos = g.Count(),
                    CompletedVideos = g.Count(x => x.IsCompleted),
                    Details = g.Select(x => new VideoProgressDetail
                    {
                        VideoId = x.VideoId,
                        Title = x.VideoTitle,
                        Category = x.VideoCategory,
                        IsCompleted = x.IsCompleted,
                        CompletedOn = x.CompletedOn
                    }).ToList()
                })
                .OrderBy(e => e.EmployeeName)
                .ToList();

            return View(grouped);
        }


        // EMPLOYEE GURUKUL PAGE
        public IActionResult EmployeeGurukul(string category)
        {
            if (HttpContext.Session.GetString("Role") != "Employee")
                return RedirectToAction("Login", "Account");

            int empId = (int)HttpContext.Session.GetInt32("EmployeeId");

            var progress = _context.GurukulProgress.Where(p => p.EmployeeId == empId).ToList();

            var model = _context.GurukulVideos
                .Select(v => new VideoProgressDetail
                {
                    VideoId = v.Id,
                    Title = v.Title,
                    Category = v.Category,
                    VideoUrl = v.VideoPath,
                    IsCompleted = progress.Any(p => p.VideoId == v.Id && p.IsCompleted)
                }).ToList();

            return View(model);
        }

        [HttpPost]
        public IActionResult MarkCompleted(int videoId)
        {
            var empId = HttpContext.Session.GetInt32("EmployeeId");

            if (empId == null)
                return Unauthorized();

            var progress = _context.GurukulProgress
                .FirstOrDefault(x => x.EmployeeId == empId && x.VideoId == videoId);

            if (progress == null)
            {
                progress = new GurukulProgress
                {
                    EmployeeId = empId.Value,
                    VideoId = videoId,
                    IsCompleted = true,
                    CompletedOn = DateTime.Now
                };
                _context.GurukulProgress.Add(progress);
            }
            else
            {
                progress.IsCompleted = true;
                progress.CompletedOn = DateTime.Now;
            }

            _context.SaveChanges();
            return Ok();
        }

        public IActionResult NextVideo(int id)
        {
            // id = current video id
            var next = _context.GurukulVideos
                .Where(v => v.Id > id)
                .OrderBy(v => v.Id)
                .FirstOrDefault();

            if (next == null)
            {
                // No next video → go back to list
                return RedirectToAction("Index");
            }

            return RedirectToAction("Watch", new { id = next.Id });
        }


        public IActionResult Watch(int id)
        {
            var video = _context.GurukulVideos.FirstOrDefault(v => v.Id == id);
            if (video == null)
                return NotFound();

            return View("ViewVideo", video);
        }
    }
}
