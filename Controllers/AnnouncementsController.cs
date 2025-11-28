using HRMS.Data;
using HRMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;

public class AnnouncementsController : Controller
{
    private readonly ApplicationDbContext _context;

    public AnnouncementsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // ==============================================
    // HR – LIST
    // ==============================================
    public async Task<IActionResult> List()
    {
        if (HttpContext.Session.GetString("Role") != "HR")
            return RedirectToAction("Login", "Account");

        var list = await _context.Announcements
            .OrderByDescending(a => a.CreatedOn)
            .ToListAsync();

        return View(list);
    }

    // ==============================================
    // HR – CREATE PAGE
    // ==============================================
    public async Task<IActionResult> Create()
    {
        ViewBag.Departments = await _context.Employees
            .Where(e => !string.IsNullOrEmpty(e.Department))
            .Select(e => e.Department)
            .Distinct()
            .OrderBy(d => d)
            .ToListAsync();

        ViewBag.Employees = await _context.Employees
            .OrderBy(e => e.Name)
            .ToListAsync();

        return View();
    }

    // ==============================================
    // HR – CREATE POST (REAL MULTI-TARGET SYSTEM)
    // ==============================================
    [HttpPost]
    public async Task<IActionResult> Create(
        string Title,
        string Message,
        bool IsUrgent,
        string targetType,
        string[]? SelectedDepartments,
        int[]? SelectedEmployees
    )
    {
        var a = new Announcement
        {
            Title = Title,
            Message = Message,
            IsUrgent = IsUrgent,
            CreatedOn = DateTime.UtcNow,
            ReadByEmployees = ""   // initially empty
        };

        if (targetType == "ALL")
        {
            a.IsGlobal = true;
        }
        else if (targetType == "DEPT")
        {
            a.TargetDepartments = SelectedDepartments != null
                ? string.Join(",", SelectedDepartments)
                : null;
        }
        else if (targetType == "EMP")
        {
            a.TargetEmployees = SelectedEmployees != null
                ? string.Join(",", SelectedEmployees)
                : null;
        }

        _context.Announcements.Add(a);
        await _context.SaveChangesAsync();

        return RedirectToAction("List");
    }

    // ==============================================
    // EMPLOYEE – LIST THEIR NOTIFICATIONS
    // ==============================================
    public async Task<IActionResult> MyNotifications()
    {
        if (HttpContext.Session.GetString("Role") != "Employee")
            return RedirectToAction("Login", "Account");

        int employeeId = HttpContext.Session.GetInt32("EmployeeId") ?? 0;

        // Pass empId to View for read-checking
        ViewBag.EmployeeId = employeeId;

        var emp = await _context.Employees.FindAsync(employeeId);
        if (emp == null)
            return RedirectToAction("Logout", "Account");

        // Load all announcements (no SQL filtering, to allow Split())
        var all = await _context.Announcements
            .OrderByDescending(a => a.CreatedOn)
            .ToListAsync();

        // In-memory filtering (supports multi department/employee)
        var list = all.Where(a =>
                a.IsGlobal ||

                (!string.IsNullOrEmpty(a.TargetDepartments) &&
                 a.TargetDepartments.Split(',')
                     .Contains(emp.Department)) ||

                (!string.IsNullOrEmpty(a.TargetEmployees) &&
                 a.TargetEmployees.Split(',')
                     .Contains(employeeId.ToString()))
            )
            .ToList();

        return View(list);
    }


    // ==============================================
    // MARK AS READ
    // ==============================================
    public async Task<IActionResult> Read(int id)
    {
        int employeeId = HttpContext.Session.GetInt32("EmployeeId") ?? 0;

        var a = await _context.Announcements.FindAsync(id);
        if (a == null)
            return RedirectToAction("MyNotifications");

        var readList = string.IsNullOrEmpty(a.ReadByEmployees)
            ? new List<string>()
            : a.ReadByEmployees.Split(',').ToList();

        if (!readList.Contains(employeeId.ToString()))
        {
            readList.Add(employeeId.ToString());
            a.ReadByEmployees = string.Join(",", readList);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("MyNotifications");
    }

    // ================================
    // DELETE ONE ANNOUNCEMENT
    // ================================
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var a = await _context.Announcements.FindAsync(id);

        if (a != null)
        {
            _context.Announcements.Remove(a);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("List");
    }

    // ================================
    // DELETE ALL ANNOUNCEMENTS
    // ================================
    [HttpGet]
    public async Task<IActionResult> DeleteAll()
    {
        var all = _context.Announcements;

        _context.Announcements.RemoveRange(all);
        await _context.SaveChangesAsync();

        return RedirectToAction("List");
    }


}
