using HRMS.Services;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Controllers
{
   


    public class AttendanceController : Controller
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        public IActionResult Index()
        {
            ViewBag.RecentRecords = _attendanceService.GetRecentAttendance();
            return View();
        }

        [HttpPost]
        public IActionResult Simulate(string jioTag, double lat, double lng)
        {
            var result = _attendanceService.MarkAttendance(jioTag, lat, lng);

            if (result == null)
                TempData["Error"] = "Out of 100 km range!";

            return RedirectToAction("Index");
        }
    }
}


