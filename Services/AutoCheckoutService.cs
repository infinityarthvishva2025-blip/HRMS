using HRMS.Data;
using Microsoft.EntityFrameworkCore;

public class AutoCheckoutService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public AutoCheckoutService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;

            // Run only at 12:00 AM
            if (now.Hour == 0 && now.Minute == 0)
            {
                await AutoCheckout();
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

    private async Task AutoCheckout()
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var yesterday = DateTime.Today.AddDays(-1);

            // Attendance records for yesterday where IN exists but OUT is missing
            var pending = await context.Attendances
                .Where(a =>
                    a.Date == yesterday &&
                    a.InTime != null &&
                    a.OutTime == null
                )
                .ToListAsync();

            foreach (var record in pending)
            {
                // Set automatic checkout time to 11:59 PM yesterday
                record.OutTime = yesterday.AddHours(23).AddMinutes(59);

                // Calculate total working hours
                if (record.InTime.HasValue)
                {
                    record.Total_Hours = record.OutTime.Value - record.InTime.Value;
                }

                // Update status (optional)
                record.Status = "Auto Checkout";
            }

            if (pending.Count > 0)
                await context.SaveChangesAsync();
        }
    }
}
