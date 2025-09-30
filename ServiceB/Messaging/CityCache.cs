using System.Collections.Concurrent;
using Microsoft.Extensions.Hosting;

namespace ServiceB.Messaging;

public interface ICityCache
{
    bool TryGetSisterCities(string city, out IReadOnlyList<string> sisterCities);
}

public class CityCache : BackgroundService, ICityCache
{
    private readonly ILogger<CityCache> _logger;
    private readonly ConcurrentDictionary<string, IReadOnlyList<string>> _sisterCities;

    public CityCache(ILogger<CityCache> logger)
    {
        _logger = logger;
        _sisterCities = new ConcurrentDictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        SeedCities();
        return Task.CompletedTask;
    }

    public bool TryGetSisterCities(string city, out IReadOnlyList<string> sisterCities)
    {
        return _sisterCities.TryGetValue(city, out sisterCities!);
    }

    private void SeedCities()
    {
        var seedData = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Bangkok"] = new[] { "Washington D.C.", "Manila", "Beijing", "Hanoi", "Jakarta" },
            ["Paris"] = new[] { "Rome", "Berlin", "Madrid", "Vienna" },
            ["New York"] = new[] { "London", "Tokyo", "Toronto", "Johannesburg" },
            ["Sydney"] = new[] { "San Francisco", "Florence", "Auckland", "Tokyo" }
        };

        foreach (var (city, sisters) in seedData)
        {
            _sisterCities[city] = sisters;
        }

        _logger.LogInformation("Seeded {Count} cities in cache", seedData.Count);
    }
}

