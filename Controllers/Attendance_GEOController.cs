using HRMS.Data;
using HRMS.Models;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Controllers
{
    public class Attendance_GEOController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly GeoFenceService _geo;

        public Attendance_GEOController(ApplicationDbContext context, GeoFenceService geo)
        {
            _context = context;
            _geo = geo;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult MarkAttendance(double latitude, double longitude)
        {
            var office = _context.OfficeLocations.First();

            double distance = _geo.Distance(
                office.Latitude,
                office.Longitude,
                latitude,
                longitude
            );

            if (distance > office.RadiusMeters)
            {
                return Json(new { success = false, message = "Outside Office" });
            }

            Attendance att = new Attendance()
            {
                Emp_Code = "EMP001",
                //CheckIn= DateTime.Now,
               
                InTime = DateTime.Now.TimeOfDay,
                GeoLatitude = latitude,
                GeoLongitude = longitude
            };

            _context.Attendances.Add(att);
            _context.SaveChanges();

            return Json(new { success = true, message = "Attendance Marked" });
        }
    }
}
