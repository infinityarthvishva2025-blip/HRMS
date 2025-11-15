using System.Net;
using System.Net.Mail;
using HRMS.Data;
using HRMS.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HRMS.Services
{
    public class EmailNotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<EmailNotificationService> _logger;

        public EmailNotificationService(
            ApplicationDbContext context,
            IConfiguration config,
            ILogger<EmailNotificationService> logger)
        {
            _context = context;
            _config = config;
            _logger = logger;
        }

        public async Task NotifyLeaveCreatedAsync(Leave leave)
        {
            var emp = await _context.Employees.FirstOrDefaultAsync(e => e.Id == leave.EmployeeId);
            if (emp == null) return;

            string subject = "Leave Request Submitted";
            string body = $"Hi {emp.Name},\n\n" +
                          $"Your leave request from {leave.StartDate:dd-MMM-yyyy} " +
                          $"to {leave.EndDate:dd-MMM-yyyy} has been submitted and is pending approval.\n\nHRMS";

            await SendEmailAsync(emp.Email, subject, body);
        }

        public async Task NotifyLeaveStatusChangedAsync(Leave leave, string stage, bool approved)
        {
            var emp = await _context.Employees.FirstOrDefaultAsync(e => e.Id == leave.EmployeeId);
            if (emp == null) return;

            string status = approved ? "Approved" : "Rejected";

            string subject = $"Leave {status} by {stage}";
            string body = $"Hi {emp.Name},\n\n" +
                          $"Your leave from {leave.StartDate:dd-MMM-yyyy} to {leave.EndDate:dd-MMM-yyyy} " +
                          $"has been {status} at {stage} level.\n\n" +
                          $"Current overall status: {leave.OverallStatus}\n\nHRMS";

            await SendEmailAsync(emp.Email, subject, body);
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var host = _config["Smtp:Host"];
                var port = int.Parse(_config["Smtp:Port"] ?? "587");
                var from = _config["Smtp:From"];
                var password = _config["Smtp:Password"];

                if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(from))
                {
                    _logger.LogWarning("SMTP not configured. Skipping email.");
                    return;
                }

                using var client = new SmtpClient(host, port)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(from, password)
                };

                using var mail = new MailMessage(from, toEmail, subject, body);
                await client.SendMailAsync(mail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email.");
            }
        }
    }
}
