using HRMS.Data;
using HRMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class HolidaysController : Controller
{
    private readonly ApplicationDbContext _context;

    public HolidaysController(ApplicationDbContext context)
    {
        _context = context;
    }

    // 🔹 COMMON SESSION CHECK METHOD
    private async Task<bool> ValidateSessionAsync()
    {
        var empId = HttpContext.Session.GetInt32("EmployeeId");
        if (!empId.HasValue)
            return false;

        var emp = await _context.Employees.FindAsync(empId.Value);
        if (emp == null)
            return false;

        ViewBag.UserRole = emp.Role?.ToString();
        return true;
    }

    // =========================
    // INDEX
    // =========================
    public async Task<IActionResult> Index()
    {
        if (!await ValidateSessionAsync())
            return RedirectToAction("Login", "Account");

        var holidays = await _context.Holidays
            .OrderBy(x => x.HolidayDate)
            .ToListAsync();

        return View(holidays);
    }

    // =========================
    // CREATE (GET)
    // =========================
    public async Task<IActionResult> Create()
    {
        if (!await ValidateSessionAsync())
            return RedirectToAction("Login", "Account");

        return View();
    }

    // =========================
    // CREATE (POST)
    // =========================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Holiday model)
    {
        if (!await ValidateSessionAsync())
            return RedirectToAction("Login", "Account");

        //if (!ModelState.IsValid)
        //    return View(model);

        // 🔥 AUTO-FILL SYSTEM FIELDS
        model.CreatedOn = DateTime.Now;
        model.Status = "Active";          // or "Approved"
        model.ApprovedBy = "Admin";           // HR/Admin later
        model.ApprovedOn = null;

        _context.Holidays.Add(model);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // =========================
    // EDIT (GET)
    // =========================
    public async Task<IActionResult> Edit(int id)
    {
        if (!await ValidateSessionAsync())
            return RedirectToAction("Login", "Account");

        var holiday = await _context.Holidays.FindAsync(id);
        if (holiday == null)
            return NotFound();

        return View(holiday);
    }

    // =========================
    // EDIT (POST)
    // =========================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Holiday model)
    {
        if (!await ValidateSessionAsync())
            return RedirectToAction("Login", "Account");

        //if (!ModelState.IsValid)
        //    return View(model);
        model.CreatedOn = DateTime.Now;
        model.Status = "Active";          // or "Approved"
        model.ApprovedBy = "Admin";
        _context.Holidays.Update(model);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // =========================
    // DELETE
    // =========================
    public async Task<IActionResult> Delete(int id)
    {
        if (!await ValidateSessionAsync())
            return RedirectToAction("Login", "Account");

        var holiday = await _context.Holidays.FindAsync(id);
        if (holiday == null)
            return NotFound();

        _context.Holidays.Remove(holiday);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // =========================
    // CALENDAR
    // =========================
    public async Task<IActionResult> Calendar()
    {
        if (!await ValidateSessionAsync())
            return RedirectToAction("Login", "Account");

        return View();
    }

    // =========================
    // CALENDAR DATA (JSON)
    // =========================
    public async Task<IActionResult> CalendarData()
    {
        if (!await ValidateSessionAsync())
            return Unauthorized();

        var data = await _context.Holidays
            .Select(x => new
            {
                title = x.HolidayName,
                start = x.HolidayDate
            })
            .ToListAsync();

        return Json(data);
    }
}
