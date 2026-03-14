using HRMS.Data;
using Microsoft.EntityFrameworkCore;

public class WorkflowService : IWorkflowService
{
    private readonly ApplicationDbContext _context;

    public WorkflowService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> GetNextApproverRoleAsync(string currentRole)
    {
        var route = await _context.LeaveApprovalRoutes
            .Where(r => r.CurrentRole == currentRole)
            .OrderBy(r => r.LevelOrder)
            .FirstOrDefaultAsync();

        return route?.NextRole ?? "Completed";
    }
}
