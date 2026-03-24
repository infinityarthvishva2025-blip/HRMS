using HRMS.Data;
using Microsoft.AspNetCore.Mvc;

public class TrackingController : Controller
{
    private readonly ApplicationDbContext _context;

    public TrackingController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Dashboard()
    {
        var routes = _context.EmployeeRoutes.ToList();

        return View(routes);
    }
}