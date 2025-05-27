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
    /// Finds all simple multi-leg routes from origin to destination using current valid pricelist.
    /// </summary>
    public async Task<List<LegOption>> FindAllRoutesAsync(string origin, string destination)
    {
        var currentPriceList = await DataInitializer.GetCurrentPriceListAsync(_db);
        if (currentPriceList == null)
        {
            return new List<LegOption>(); // No valid pricelist available
        }
        
        var legs = await _db.Legs
            .Include(l => l.Providers)
            .Where(l => l.PriceListId == currentPriceList.Id)
            .ToListAsync();
        var paths = FindPaths(origin, destination, legs);
        var options = new List<LegOption>();
        foreach (var path in paths)
        {
            // Generate multiple combinations of providers for each path
            var pathOptions = GenerateProviderCombinations(path, legs);
            options.AddRange(pathOptions);
        }
        
        // Sort by price and return top results
        return options.OrderBy(o => o.TotalPrice).Take(50).ToList();
    }
    
    private List<LegOption> GenerateProviderCombinations(List<string> path, List<Leg> legs)
    {
        var combinations = new List<LegOption>();
        var legProviders = new List<List<Provider>>();
        
        // Get all providers for each leg in the path
        for (int i = 0; i < path.Count - 1; i++)
        {
            var from = path[i];
            var to = path[i + 1];
            var providers = legs
                .Where(l => l.From == from && l.To == to)
                .SelectMany(l => l.Providers)
                .ToList();
            if (!providers.Any())
                return combinations; // No valid route
            legProviders.Add(providers);
        }
        
        // Generate combinations (limited to prevent explosion)
        var maxCombinations = 20;
        var currentCombinations = 0;
        
        GenerateCombinationsRecursive(legProviders, 0, new List<Provider>(), combinations, path, ref currentCombinations, maxCombinations);
        
        return combinations;
    }
    
    private void GenerateCombinationsRecursive(List<List<Provider>> legProviders, int legIndex, 
        List<Provider> currentSelection, List<LegOption> combinations, List<string> path, 
        ref int currentCombinations, int maxCombinations)
    {
        if (currentCombinations >= maxCombinations) return;
        
        if (legIndex >= legProviders.Count)
        {
            // Complete combination found
            var routeDesc = string.Join(" -> ", path);
            var companies = string.Join(", ", currentSelection.Select(p => p.CompanyName).Distinct());
            var price = currentSelection.Sum(p => p.Price);
            var distance = currentSelection.Sum(p => p.Leg!.Distance);
            var travelTime = currentSelection.Aggregate(TimeSpan.Zero,
                (sum, p) => sum + (p.FlightEnd - p.FlightStart));
            var providerIds = currentSelection.Select(p => p.Id).ToList();
            
            combinations.Add(new LegOption
            {
                RouteDescription = routeDesc,
                CompanyNames = companies,
                TotalPrice = price,
                TotalDistance = distance,
                TotalTravelTime = travelTime,
                ProviderIds = providerIds
            });
            currentCombinations++;
            return;
        }
        
        // Try each provider for current leg (limit to top 5 by price)
        var providersForLeg = legProviders[legIndex].OrderBy(p => p.Price).Take(5);
        foreach (var provider in providersForLeg)
        {
            if (currentCombinations >= maxCombinations) break;
            currentSelection.Add(provider);
            GenerateCombinationsRecursive(legProviders, legIndex + 1, currentSelection, combinations, path, ref currentCombinations, maxCombinations);
            currentSelection.RemoveAt(currentSelection.Count - 1);
        }
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