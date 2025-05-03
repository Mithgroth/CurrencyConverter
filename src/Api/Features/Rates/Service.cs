using Api.Providers;
using Domain;

namespace Api.Features.Rates;

public class Service(IExchangeRateProvider provider)
{
    public async Task<List<ExchangeRatesResponse>> GetPagedAsync(
        string baseCurrency, DateOnly from, DateOnly to,
        int page, int pageSize,
        CancellationToken cancellationToken)
    {
        var businessDays = GetBusinessDays(from, to);
        var pagedDays = businessDays
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        if (!pagedDays.Any())
        {
            return [];
        }

        var min = pagedDays.First();
        var max = pagedDays.Last();

        var domain = await provider.GetHistorical(
            new Currency(baseCurrency), min, max, cancellationToken);

        return domain
            .Where(r => pagedDays.Contains(r.Date))
            .OrderBy(r => r.Date)
            .Select(r => new ExchangeRatesResponse(r))
            .ToList();
    }

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
