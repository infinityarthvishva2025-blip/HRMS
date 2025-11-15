using HRMS.Models;

public interface IWorkflowService
{
    Task<string> GetNextApproverRoleAsync(string currentRole);
}