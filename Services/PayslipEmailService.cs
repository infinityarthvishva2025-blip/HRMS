using MailKit.Net.Smtp;
using MimeKit;
using System.Net.Mail;

namespace HRMS.Services
{
    public class PayslipEmailService
    {
        private readonly IConfiguration _config;

        public PayslipEmailService(IConfiguration config)
        {
            _config = config;
        }

        public void SendPayslip(string toEmail, string employeeName, string subject, string body, byte[] pdfBytes, string fileName)
        {
            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress("HRMS Payroll", _config["Smtp:From"]));
            msg.To.Add(MailboxAddress.Parse(toEmail));
            msg.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = body
            };

            builder.Attachments.Add(fileName, pdfBytes, ContentType.Parse("application/pdf"));

            msg.Body = builder.ToMessageBody();

           // using var client = new SmtpClient();
            //client.Connect(_config["Smtp:Host"], int.Parse(_config["Smtp:Port"]), true);
            //client.Authenticate(_config["Smtp:User"], _config["Smtp:Pass"]);
            //client.Send(msg);
            //client.Disconnect(true);
        }
    }
}
