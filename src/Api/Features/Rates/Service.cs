using Api.Providers;
using Domain;

namespace Api.Features.Rates;

public class Service(Resolver resolver)
{
    public async Task<ExchangeRates> GetLatest(string baseCurrency, string providerName, CancellationToken ct)
    {
        var provider = resolver.Get(providerName);
        return await provider.GetLatest(new Currency(baseCurrency), ct);
    }

    public async Task<List<ExchangeRates>> GetHistorical(string baseCurrency, DateOnly from, DateOnly to, string providerName, CancellationToken ct)
    {
        var provider = resolver.Get(providerName);
        return await provider.GetHistorical(new Currency(baseCurrency), from, to, ct);
    }

    // TODO: You stay for pagination
    private static List<DateOnly> GetBusinessDays(DateOnly from, DateOnly to)
    {
        var days = new List<DateOnly>();
        for (var d = from; d <= to; d = d.AddDays(1))
        {
            if (d.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday)
            {
                days.Add(d);
            }
        }

        return days;
    }
}
