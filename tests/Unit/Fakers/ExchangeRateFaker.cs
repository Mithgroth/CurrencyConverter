using Bogus;

namespace Unit.Fakers;

public sealed class ExchangeRatesFaker : Faker<Domain.ExchangeRates>
{
    public ExchangeRatesFaker()
    {
        var currencyFaker = new CurrencyFaker();

        RuleFor(x => x.BaseCurrency, _ => currencyFaker.Generate());

        RuleFor(x => x.Date, f =>
            DateOnly.FromDateTime(f.Date.Past()));

        RuleFor(x => x.Rates, f =>
        {
            var currencies = Enumerable.Range(0, 5)
                .Select(_ => currencyFaker.Generate())
                .DistinctBy(c => c.Code)
                .ToList();

            return currencies.ToDictionary(
                c => c,
                _ => f.Random.Decimal(0.5m, 50m));
        });
    }
}
