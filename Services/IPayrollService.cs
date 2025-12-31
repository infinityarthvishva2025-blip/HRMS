
namespace HRMS.Services
{
    public interface IPayrollService
    {
        Task GeneratePayrollAsync(int month, int year,DateTime FromDate, DateTime ToDate);
        bool IsPayrollLocked(int month, int year);
        void LockPayroll(int month, int year, string user);
    }
}