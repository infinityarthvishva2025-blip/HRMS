using HRMS.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public class AutoCheckoutService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public AutoCheckoutService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
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

    public async Task AutoCheckout()
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var yesterday = DateTime.Today.AddDays(-1);

            // Attendance records for yesterday where IN exists but OUT is missing
            var pending = await context.Attendances
                .Where(a =>
                    a.Date == yesterday &&
                    a.InTime != null &&
                    a.OutTime == null)
                .ToListAsync();

            foreach (var record in pending)
            {
                // Set automatic checkout time to 11:59 PM (time-of-day)
                record.OutTime = new TimeSpan(23, 59, 0);

                // Calculate total working hours
                if (record.InTime.HasValue && record.OutTime.HasValue)
                {
                    TimeSpan diff = record.OutTime.Value - record.InTime.Value;
                    record.Total_Hours = (decimal)diff.TotalHours;
                }

                // Update status (optional)
                record.Status = "Auto Checkout";
            }

            if (pending.Count > 0)
                await context.SaveChangesAsync();
        }
    }

}

