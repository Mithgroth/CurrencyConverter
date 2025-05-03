using System.Text.Json;
using Domain;
using Microsoft.Extensions.Caching.Memory;

namespace Api.Providers.Frankfurter;

public class Provider(IHttpClientFactory httpClientFactory, IMemoryCache cache) : IExchangeRateProvider
{
    public string Name => "Frankfurter";

    public async Task<ExchangeRates> GetLatest(
        Currency baseCurrency,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"latest_{baseCurrency.Code}";
        if (cache.TryGetValue(cacheKey, out ExchangeRates cached))
        {
            return cached;
        }

        var httpClient = httpClientFactory.CreateClient("Frankfurter");
        var url = $"/v1/latest?base={baseCurrency.Code}";

        var response = await httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var parsed = JsonSerializer
            .Deserialize<FrankfurterLatestResponse>(json, FrankfurterJsonOptions.Options)!
            .ToDomain();

        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };
        cache.Set(cacheKey, parsed, options);

        return parsed;
    }

    public async Task<List<ExchangeRates>> GetHistorical(
        Currency baseCurrency,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"historical_{baseCurrency.Code}_{from:yyyy-MM-dd}_{to:yyyy-MM-dd}";
        if (cache.TryGetValue(cacheKey, out List<ExchangeRates> cached))
        {
            return cached;
        }

        var httpClient = httpClientFactory.CreateClient("Frankfurter");
        var url = $"/v1/{from:yyyy-MM-dd}..{to:yyyy-MM-dd}?base={baseCurrency.Code}";

        var response = await httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var parsed = JsonSerializer
            .Deserialize<FrankfurterHistoricalRatesResponse>(json, FrankfurterJsonOptions.Options)!
            .ToDomain();

        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
        };
        cache.Set(cacheKey, parsed, options);

        return parsed;
    }
}
