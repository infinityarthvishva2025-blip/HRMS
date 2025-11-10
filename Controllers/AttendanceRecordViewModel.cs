using Microsoft.AspNetCore.Mvc;

namespace HRMS.Controllers
{
    public class AttendanceRecordViewModel : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
