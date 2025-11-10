using Microsoft.AspNetCore.Mvc;
using HRMS.Data;
using HRMS.Models;
using HRMS.Services;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Controllers
{
    [Route("[controller]")]
    public class GeoAttendanceController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly FaceService _faceService;
        private readonly IWebHostEnvironment _env;

        public GeoAttendanceController(ApplicationDbContext db, FaceService faceService, IWebHostEnvironment env)
        {
            _db = db;
            _faceService = faceService;
            _env = env;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var model = new ViewModels.GeoTagViewModel
            {
                GeoTags = await _db.GeoTags.AsNoTracking().Select(g => new KeyValuePair<int, string>(g.Id, g.TagId)).ToListAsync()
            };
            return View(model);
        }

        // API: Create attendance (client sends lat,long, geotagId and a selfie file)
        [HttpPost("api/mark")]
        public async Task<IActionResult> MarkAttendance([FromForm] AttendanceRequest request)
        {
            // validate geotag exists
            var geo = await _db.GeoTags.FindAsync(request.GeoTagId);
            if (geo == null) return BadRequest("Invalid geotag");

            // check distance
            var distanceM = HaversineDistanceMeters(geo.Latitude, geo.Longitude, request.Latitude, request.Longitude);
            var withinRange = distanceM <= geo.RadiusMeters;

            // Save selfie temporarily
            string? verificationResponse = null;
            bool faceVerified = false;

            if (request.Selfie != null && request.Selfie.Length > 0)
            {
                using var ms = new MemoryStream();
                await request.Selfie.CopyToAsync(ms);
                ms.Position = 0;

                // Detect face, get faceIds
                var faceIds = await _faceService.DetectFacesAsync(ms);

                if (faceIds.Count > 0)
                {
                    // For demo: compare detected face with a stored face for the user (you'd store faceId or persisted sample)
                    // Here we simply consider presence of 1 face as verification (or you can call Verify with a storedFaceId)
                    faceVerified = true;
                    verificationResponse = $"Detected {faceIds.Count} face(s)";
                }
                else
                {
                    verificationResponse = "No face detected";
                }
            }

            var attendance = new Attendance
            {
                EmployeeId = request.EmployeeId ?? "unknown",
                GeoTagId = request.GeoTagId,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                FaceVerified = faceVerified && withinRange,
                FaceVerificationResponse = verificationResponse
            };

            _db.Attendances.Add(attendance);
            await _db.SaveChangesAsync();

            return Ok(new { success = true, withinRange, faceVerified, verificationResponse });
        }

        // auxiliary request model
        public class AttendanceRequest
        {
            public string? EmployeeId { get; set; }
            public int GeoTagId { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public IFormFile? Selfie { get; set; }
        }

        // Haversine formula (meters)
        private static double HaversineDistanceMeters(double lat1, double lon1, double lat2, double lon2)
        {
            double R = 6371000; // meters
            var φ1 = ToRadians(lat1);
            var φ2 = ToRadians(lat2);
            var Δφ = ToRadians(lat2 - lat1);
            var Δλ = ToRadians(lon2 - lon1);

            var a = Math.Sin(Δφ / 2) * Math.Sin(Δφ / 2) +
                    Math.Cos(φ1) * Math.Cos(φ2) *
                    Math.Sin(Δλ / 2) * Math.Sin(Δλ / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private static double ToRadians(double deg) => deg * (Math.PI / 180.0);
    }
}


////namespace HRMS.Controllers
////{
////    public class GeoAttendanceController
////    {
////    }
////}
//using HRMS.Models;
//using Microsoft.AspNetCore.Mvc;

//namespace HRMS.Controllers
//{
//    public class GeoAttendanceController : Controller
//    {
//        public IActionResult Index()
//        {
//            var model = new GeoTagViewModel
//            {
//                GeoTags = new List<string> { "Office-001", "Factory-002", "Site-003" },
//                CurrentLatitude = 19.0760,
//                CurrentLongitude = 72.8777,
//                CurrentCity = "Mumbai, India"
//            };

//            return View(model);
//        }
//    }
//}
