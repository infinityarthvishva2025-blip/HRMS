using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HRMS.Services
{
    public class CompOffExpiryHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public CompOffExpiryHostedService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        //protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        //{
        //    // run every day
        //    while (!stoppingToken.IsCancellationRequested)
        //    {
        //        using var scope = _scopeFactory.CreateScope();
        //        var service = scope.ServiceProvider.GetRequiredService<ICompOffService>();

        //      //  await service.ExpireOldCompOffAsync(DateTime.Today);

        //        // Sleep 24 hours
        //        await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        //    }
        //}
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var service = scope.ServiceProvider.GetRequiredService<ICompOffService>();

                    await service.ExpireOldCompOffAsync(DateTime.Today);
                }
                catch (Exception ex)
                {
                    // optional: log error
                }

                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }

    }
}
