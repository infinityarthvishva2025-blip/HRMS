namespace HRMS.Services
{
    //public interface IEmailService
    //{
    //    Task SendEmailWithAttachmentAsync(
    //        string toEmail,
    //        string subject,
    //        string htmlBody,
    //        string? attachmentFilePath
    //    );
    //}
    public interface IEmailService
    {
        //Task SendRelievingLetterEmail(string toEmail, string employeeName, string filePath);
        Task SendRelievingLetterEmail(string email, string employeeName, string filePath);

        Task SendRelievingAndExperienceEmail(
            string email,
            string employeeName,
            string relievingPath
            //string experiencePath
        );
    }

}