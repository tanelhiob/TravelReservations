using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace TravelReservations.Data;

public static class DataInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();
        if (await context.Legs.AnyAsync())
        {
            return;   // DB has been seeded
        }

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

        foreach (var legDto in dto.Legs)
        {
            var leg = new Leg
            {
                Id = legDto.Id,
                From = legDto.RouteInfo.From.Name,
                To = legDto.RouteInfo.To.Name,
                Distance = legDto.RouteInfo.Distance
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
            context.Legs.Add(leg);
        }

        await context.SaveChangesAsync();
    }

    private record TravelPricesDto(Guid Id, DateTime ValidUntil, List<LegDto> Legs);
    private record LegDto(Guid Id, RouteInfoDto RouteInfo, List<ProviderDto> Providers);
    private record RouteInfoDto(LocationDto From, LocationDto To, long Distance);
    private record LocationDto(Guid Id, string Name);
    private record ProviderDto(Guid Id, CompanyDto Company, decimal Price, DateTime FlightStart, DateTime FlightEnd);
    private record CompanyDto(Guid Id, string Name);
}