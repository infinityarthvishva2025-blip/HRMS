using HRMS.Models;

namespace HRMS.Services
{
    public interface INotificationService
    {
        Task NotifyLeaveCreatedAsync(Leave leave);
        Task NotifyLeaveStatusChangedAsync(Leave leave, string stage, bool approved);
    }
}