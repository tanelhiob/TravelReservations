using Microsoft.EntityFrameworkCore;

namespace TravelReservations.Data;

public class LegOption
{
    public string RouteDescription { get; set; } = string.Empty;
    public string CompanyNames { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public long TotalDistance { get; set; }
    public TimeSpan TotalTravelTime { get; set; }
    public List<Guid> ProviderIds { get; set; } = new();
}

public class RouteSearchService
{
    private readonly AppDbContext _db;

    public RouteSearchService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Finds all simple multi-leg routes from origin to destination.
    /// </summary>
    public async Task<List<LegOption>> FindAllRoutesAsync(string origin, string destination)
    {
        var legs = await _db.Legs.Include(l => l.Providers).ToListAsync();
        var paths = FindPaths(origin, destination, legs);
        var options = new List<LegOption>();
        foreach (var path in paths)
        {
            var selectedProviders = new List<Provider>();
            bool valid = true;
            for (int i = 0; i < path.Count - 1; i++)
            {
                var from = path[i];
                var to = path[i + 1];
                var providers = legs
                    .Where(l => l.From == from && l.To == to)
                    .SelectMany(l => l.Providers);
                if (!providers.Any()) { valid = false; break; }
                var best = providers.OrderBy(p => p.Price).First();
                selectedProviders.Add(best);
            }
            if (!valid) continue;
            var routeDesc = string.Join(" -> ", path);
            var companies = string.Join(", ", selectedProviders.Select(p => p.CompanyName).Distinct());
            var price = selectedProviders.Sum(p => p.Price);
            var distance = selectedProviders.Sum(p => p.Leg!.Distance);
            var travelTime = selectedProviders.Aggregate(TimeSpan.Zero,
                (sum, p) => sum + (p.FlightEnd - p.FlightStart));
            var providerIds = selectedProviders.Select(p => p.Id).ToList();
            options.Add(new LegOption
            {
                RouteDescription = routeDesc,
                CompanyNames = companies,
                TotalPrice = price,
                TotalDistance = distance,
                TotalTravelTime = travelTime,
                ProviderIds = providerIds
            });
        }
        return options;
    }

    // Depth-first search to enumerate simple paths
    private List<List<string>> FindPaths(string origin, string destination, List<Leg> legs)
    {
        var result = new List<List<string>>();
        var visited = new HashSet<string>();
        var path = new List<string> { origin };
        void Dfs(string current)
        {
            if (current == destination)
            {
                result.Add(new List<string>(path));
                return;
            }
            visited.Add(current);
            foreach (var leg in legs.Where(l => l.From == current))
            {
                if (!visited.Contains(leg.To))
                {
                    path.Add(leg.To);
                    Dfs(leg.To);
                    path.RemoveAt(path.Count - 1);
                }
            }
            visited.Remove(current);
        }
        Dfs(origin);
        return result;
    }
}