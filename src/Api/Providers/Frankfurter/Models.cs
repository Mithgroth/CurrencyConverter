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
