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
            var yesterdayStart = yesterday.Date;
            var yesterdayEnd = yesterdayStart.AddDays(1);

            // Employees who checked-in yesterday but DID NOT checkout
            var pending = await context.Attendances
                .Where(a =>
                    a.CheckInTime >= yesterdayStart &&
                    a.CheckInTime < yesterdayEnd &&
                    a.CheckOutTime == null
                )
                .ToListAsync();

            foreach (var record in pending)
            {
                record.CheckOutTime = yesterdayStart.AddHours(23).AddMinutes(59); // 11:59 PM
                record.CheckoutStatus = "Auto Checkout";

                var diff = record.CheckOutTime.Value - record.CheckInTime.Value;
                record.WorkingHours = diff.TotalHours;
            }

            if (pending.Count > 0)
                await context.SaveChangesAsync();
        }

    }
}
