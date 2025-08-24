using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace TravelReservations.Data;

public static class DataInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();
        await FetchAndStorePriceListAsync(serviceProvider);
    }

    public static async Task FetchAndStorePriceListAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();
        var httpFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        
        var client = httpFactory.CreateClient("TravelApi");
        var response = await client.GetAsync("TravelPrices");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };
        var dto = JsonSerializer.Deserialize<TravelPricesDto>(json, options);
        if (dto?.Legs == null)
            return;

        // Check if this pricelist already exists
        var existingPriceList = await context.PriceLists
            .FirstOrDefaultAsync(p => p.Id == dto.Id);
        if (existingPriceList != null)
            return; // Already stored

        var priceList = new PriceList
        {
            Id = dto.Id,
            ValidUntil = dto.ValidUntil,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var legDto in dto.Legs)
        {
            var leg = new Leg
            {
                Id = legDto.Id,
                From = legDto.RouteInfo.From.Name,
                To = legDto.RouteInfo.To.Name,
                Distance = legDto.RouteInfo.Distance,
                PriceListId = priceList.Id
            };
            foreach (var provDto in legDto.Providers)
            {
                leg.Providers.Add(new Provider
                {
                    Id = provDto.Id,
                    CompanyName = provDto.Company.Name,
                    Price = provDto.Price,
                    FlightStart = provDto.FlightStart,
                    FlightEnd = provDto.FlightEnd
                });
            }
            priceList.Legs.Add(leg);
        }

        context.PriceLists.Add(priceList);
        await context.SaveChangesAsync();

        // Clean up old pricelists - keep only last 15
        await CleanupOldPriceListsAsync(context);
    }

    private static async Task CleanupOldPriceListsAsync(AppDbContext context)
    {
        var allPriceLists = await context.PriceLists
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        if (allPriceLists.Count > 15)
        {
            var toRemove = allPriceLists.Skip(15).ToList();
            foreach (var priceList in toRemove)
            {
                // Remove related legs and providers first
                var legs = await context.Legs.Where(l => l.PriceListId == priceList.Id).ToListAsync();
                foreach (var leg in legs)
                {
                    var providers = await context.Providers.Where(p => p.LegId == leg.Id).ToListAsync();
                    context.Providers.RemoveRange(providers);
                }
                context.Legs.RemoveRange(legs);
                context.PriceLists.Remove(priceList);
            }
            await context.SaveChangesAsync();
        }
    }

    public static async Task<PriceList?> GetCurrentPriceListAsync(AppDbContext context)
    {
        return await context.PriceLists
            .Where(p => p.ValidUntil > DateTime.UtcNow)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync();
    }
}

public record TravelPricesDto(Guid Id, DateTime ValidUntil, List<LegDto> Legs);
public record LegDto(Guid Id, RouteInfoDto RouteInfo, List<ProviderDto> Providers);
public record RouteInfoDto(LocationDto From, LocationDto To, long Distance);
public record LocationDto(Guid Id, string Name);
public record ProviderDto(Guid Id, CompanyDto Company, decimal Price, DateTime FlightStart, DateTime FlightEnd);
public record CompanyDto(Guid Id, string Name);