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

    public async Task<List<ExchangeRates>> GetHistorical(
        string baseCurrency,
        DateOnly from,
        DateOnly to,
        int page,
        int pageSize,
        string providerName,
        CancellationToken ct)
    {
        if (page <= 0 || pageSize <= 0)
        {
            throw new ArgumentException("Page and pageSize must be greater than zero.");
        }

        var provider = resolver.Get(providerName);
        var allBusinessDays = GetBusinessDays(from, to);

        var skip = (page - 1) * pageSize;
        var calculatedBusinessDays = allBusinessDays.Skip(skip).Take(pageSize).ToList();
        if (calculatedBusinessDays?.Count == 0)
        {
            return [];
        }

        return await provider.GetHistorical(
            new Currency(baseCurrency),
            calculatedBusinessDays.First(),
            calculatedBusinessDays.Last(),
            ct);
    }

    private static List<DateOnly> GetBusinessDays(DateOnly from, DateOnly to)
    {
        if (from.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            var lastBusinessDay = from.DayOfWeek == DayOfWeek.Saturday ? 1 : 2;
            from = from.AddDays(-lastBusinessDay);
        }

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
