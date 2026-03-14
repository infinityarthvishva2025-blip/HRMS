using DocumentFormat.OpenXml.InkML;
using HRMS.Data;
using HRMS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class AdminLetterController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IEmailService _emailService;

    public AdminLetterController(ApplicationDbContext db, IEmailService emailService)
    {
        _db = db;
        _emailService = emailService;
    }

    public IActionResult Index()
    {
        var letters = _db.LetterHistory
            .Include(l => l.Employee)
            .OrderByDescending(l => l.SentDate)
            .ToList();

        return View(letters);
    }
    public async Task<IActionResult> Resend(int id)
    {
        var letter = await _db.LetterHistory.FirstOrDefaultAsync(x => x.Id == id);

        if (letter == null)
            return NotFound();

        var employee = await _db.Employees.FindAsync(letter.EmployeeId);

        await _emailService.SendRelievingLetterEmail(
            employee.Email,
            employee.Name,
            letter.FilePath
        );

        letter.Status = "Re-Sent";
        letter.SentDate = DateTime.Now;

        await _db.SaveChangesAsync();

        return RedirectToAction("Index");
    }
}