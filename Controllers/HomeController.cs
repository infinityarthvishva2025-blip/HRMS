using HRMS.Models;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var model = new DashboardViewModel
            {
                TotalEmployees = 47,
                TodaysPresent = 42,
                GeoTagsAssigned = 35,
                PendingLeaves = 8,
                LastUpdated = DateTime.Now
            };

            return View(model);
        }
    }
}