using HRMS.Models;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace HRMS.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly SmtpSettings _settings;

        public SmtpEmailService(IOptions<SmtpSettings> options)
        {
            _settings = options.Value;
        }

        public async Task SendRelievingLetterEmail(string email, string employeeName, string filePath)
        {
            var message = new MailMessage
            {
                From = new MailAddress(_settings.FromEmail, _settings.FromName),
                Subject = "Relieving Letter - HRMS",
                Body = $@"Dear {employeeName},

Your relieving letter is attached.

Regards,
HR Team"
            };

            message.To.Add(email);
            message.Attachments.Add(new Attachment(filePath));

            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                EnableSsl = _settings.EnableSsl
            };

            await client.SendMailAsync(message);
        }

        public async Task SendRelievingAndExperienceEmail(string email, string employeeName, string filePath)
        {
            var message = new MailMessage
            {
                From = new MailAddress(_settings.FromEmail, _settings.FromName),
                Subject = "Relieving Letter - HRMS",
                Body = $@"Dear {employeeName},

Please find attached your Relieving  Letter.

Regards,
HR Team"
            };

            message.To.Add(email);

            message.Attachments.Add(new Attachment(filePath));

            using (var smtp = new SmtpClient(_settings.Host, _settings.Port))
            {
                smtp.Credentials = new NetworkCredential(_settings.Username, _settings.Password);
                smtp.EnableSsl = true;

                await smtp.SendMailAsync(message);
            }
        }
    }
}