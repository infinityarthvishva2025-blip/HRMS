using HRMS.Data;
using HRMS.Models;
using HRMS.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class RelievingLetterJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public RelievingLetterJob(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                var pdfService = scope.ServiceProvider.GetRequiredService<IPdfService>();

                var today = DateTime.Today;

                //var resignations = await db.ResignationRequests
                //    .Include(x => x.Employee)
                //    .Where(x => x.LastWorkingDate.HasValue 
                //    && x.LastWorkingDate.Value.Date == today
                //                && x.Status == ResignationStatus.InApproval
                //                && !x.RelievingLetterSent)
                //    .ToListAsync(stoppingToken);
                var resignations = await db.ResignationRequests
  //  .Include(x => x.Employee)
    .Where(x =>
        x.LastWorkingDate.HasValue &&
        x.LastWorkingDate.Value.Date == today &&
        x.Status == ResignationStatus.InApproval &&
        x.RelievingLetterSent != true
    )
    .ToListAsync(stoppingToken);

                //foreach (var r in resignations)
                //{
                //    var employee = r.Employee;

                //    // Generate PDF
                //    var pdfBytes = pdfService.GenerateRelievingLetterPdf(employee);

                //    var folder = Path.Combine("wwwroot", "generated", "letters");
                //    Directory.CreateDirectory(folder);

                //    var fileName = $"relieving_{employee.Id}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                //    var filePath = Path.Combine(folder, fileName);

                //    await File.WriteAllBytesAsync(filePath, pdfBytes, stoppingToken);

                //    // Send Email
                //    await emailService.SendEmailWithAttachmentAsync(
                //        employee.Email,
                //        "Relieving Letter",
                //        $"Dear {employee.Name},<br/>Please find attached your relieving letter.",
                //        filePath
                //    );

                //    // Mark letter sent
                //    r.RelievingLetterSent = true;
                //}
                foreach (var r in resignations)
                {
                    var employee = r.Employee;

                    var pdfBytes = pdfService.GenerateRelievingLetterPdf(employee);

                    var folder = Path.Combine("wwwroot", "generated", "letters");
                    Directory.CreateDirectory(folder);

                    var fileName = $"relieving_{employee.Id}_{DateTime.Now:yyyyMMddHHmmss}.pdf";

                    var filePath = Path.Combine(folder, fileName);

                    await File.WriteAllBytesAsync(filePath, pdfBytes);

                    // Send Email
                    await emailService.SendRelievingLetterEmail(
                        employee.Email,
                        employee.Name,
                        filePath
                    );

                    r.RelievingLetterGenerated = true;
                }

                await db.SaveChangesAsync();
                await db.SaveChangesAsync(stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // Application is shutting down - exit loop safely
                break;
            }
            catch (Exception ex)
            {
                // Log real error
                Console.WriteLine("RelievingLetterJob Error: " + ex.Message);
            }

            try
            {
                // Wait 24 hours
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }
}