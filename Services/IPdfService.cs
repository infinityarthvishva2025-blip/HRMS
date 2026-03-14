using HRMS.Models;

namespace HRMS.Services
{
    public interface IPdfService
    {
        byte[] GenerateRelievingLetterPdf(Employee employee);
    }
}