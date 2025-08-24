using Microsoft.EntityFrameworkCore;

namespace TravelReservations.Data;

public class PriceListBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PriceListBackgroundService> _logger;

    public PriceListBackgroundService(IServiceProvider serviceProvider, ILogger<PriceListBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                
                var currentPriceList = await DataInitializer.GetCurrentPriceListAsync(context);
                
                if (currentPriceList == null || currentPriceList.ValidUntil <= DateTime.UtcNow.AddMinutes(5))
                {
                    _logger.LogInformation("Fetching new pricelist...");
                    await DataInitializer.FetchAndStorePriceListAsync(scope.ServiceProvider);
                    _logger.LogInformation("New pricelist fetched successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching pricelist");
            }

            // Check every 5 minutes
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}