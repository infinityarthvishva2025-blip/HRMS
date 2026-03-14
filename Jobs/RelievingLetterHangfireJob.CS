using HRMS.Data;
using HRMS.Models;
using HRMS.Services;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Jobs
{
    public class RelievingLetterHangfireJob
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmailService _emailService;
        private readonly IPdfService _pdfService;
        private readonly IWebHostEnvironment _env;
        private readonly ExperiencePdfService _experiencePdfService;
        public RelievingLetterHangfireJob(
    ApplicationDbContext db,
    IPdfService pdfService,
    ExperiencePdfService experiencePdfService,
    IEmailService emailService,
    IWebHostEnvironment env)
        {
            _db = db;
            _pdfService = pdfService;
            _experiencePdfService = experiencePdfService;
            _emailService = emailService;
            _env = env;
        }

        public async Task ExecuteAsync()
        {
            var today = DateTime.Today;

            Console.WriteLine($"Relieving Letter Job Started: {today}");

            var resignations = await _db.ResignationRequests
                .Include(x => x.Employee)
                .Where(x =>
                    x.LastWorkingDate.HasValue &&
                    x.LastWorkingDate.Value.Date == today &&
                    x.Status == ResignationStatus.Approved &&
                    !x.RelievingLetterGenerated
                )
                .ToListAsync();

            if (!resignations.Any())
            {
                Console.WriteLine("No relieving letters to generate today.");
                return;
            }

            foreach (var r in resignations)
            {
                try
                {
                    var employee = r.Employee;

                    if (employee == null)
                        continue;

                    if (string.IsNullOrWhiteSpace(employee.Email))
                        continue;

                    Console.WriteLine($"Generating letters for {employee.Name}");

                    // Generate Relieving Letter
                    var relievingBytes = _pdfService.GenerateRelievingLetterPdf(employee);

                    // Generate Experience Letter
                   // var experienceBytes = _experiencePdfService.GenerateExperienceLetter(employee);

                    var folder = Path.Combine(_env.WebRootPath, "letters");

                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);

                    var relievingPath = Path.Combine(folder, $"relieving_{employee.Id}.pdf");
                   // var experiencePath = Path.Combine(folder, $"experience_{employee.Id}.pdf");

                    await File.WriteAllBytesAsync(relievingPath, relievingBytes);
                   // await File.WriteAllBytesAsync(experiencePath, experienceBytes);

                    // Send Email with Both Attachments
                    await _emailService.SendRelievingAndExperienceEmail(
                        employee.Email,
                        employee.Name,
                        relievingPath
                      //  experiencePath
                    );

                    // Update database flags
                    r.RelievingLetterGenerated = true;
                    r.RelievingLetterSent = true;

                    _db.LetterHistory.Add(new LetterHistory
                    {
                        EmployeeId = employee.Id,
                        LetterType = "Relieving ",
                        FilePath = relievingPath,
                        SentToEmail = employee.Email,
                        SentDate = DateTime.Now,
                        SentBy = "System",
                        Status = "Sent"
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error for employee {r.EmployeeId}: {ex.Message}");
                }
            }

            await _db.SaveChangesAsync();

            await _db.SaveChangesAsync();
            await _db.SaveChangesAsync();
            Console.WriteLine("Relieving Letter Job Completed");
        }
    }
}