using Domain;

namespace Api.Providers.Frankfurter;

public record FrankfurterLatestResponse(string Base, DateOnly Date, Dictionary<string, decimal> Rates)
{
    public ExchangeRates ToDomain() =>
        new(
            baseCurrency: new Currency(Base),
            date: Date,
            rates: Rates.ToDictionary(kvp => new Currency(kvp.Key), kvp => kvp.Value));
}

public sealed record FrankfurterHistoricalRatesResponse(
    string Base,
    DateOnly StartDate,
    DateOnly EndDate,
    Dictionary<string, Dictionary<string, decimal>> Rates)
{
    public List<ExchangeRates> ToDomain()
    {
        var baseCurrency = new Currency(Base);

        return Rates.Select(entry =>
        {
            var date = DateOnly.Parse(entry.Key);
            var rates = entry.Value.ToDictionary(
                kvp => new Currency(kvp.Key),
                kvp => kvp.Value);

            return new ExchangeRates(baseCurrency, date, rates);
        }).OrderBy(x => x.Date).ToList();
    }
}
