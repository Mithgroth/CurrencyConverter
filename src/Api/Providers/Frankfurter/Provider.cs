using System.Text.Json;
using Domain;

namespace Api.Providers.Frankfurter;

public class Provider(IHttpClientFactory httpClientFactory) : IExchangeRateProvider
{
    public async Task<ExchangeRates> GetLatest(Currency baseCurrency, CancellationToken cancellationToken = default)
    {
        var httpClient = httpClientFactory.CreateClient("Frankfurter");
        var url = $"/v1/latest?base={baseCurrency.Code}";

        var response = await httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var parsed = JsonSerializer.Deserialize<FrankfurterLatestResponse>(json, FrankfurterJsonOptions.Options)!;
        return parsed.ToDomain();
    }

    public async Task<List<ExchangeRates>> GetHistorical(
        Currency baseCurrency,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default)
    {
        var httpClient = httpClientFactory.CreateClient("Frankfurter");
        var url = $"/v1/{from:yyyy-MM-dd}..{to:yyyy-MM-dd}?base={baseCurrency.Code}";
        var response = await httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var parsed = JsonSerializer.Deserialize<FrankfurterHistoricalRatesResponse>(json, FrankfurterJsonOptions.Options)!;
        return parsed.ToDomain();
    }
}
