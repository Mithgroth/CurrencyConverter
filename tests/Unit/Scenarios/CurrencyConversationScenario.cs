using Bogus;
using Domain;
using Unit.Fakers;

namespace Unit.Scenarios;

public static class CurrencyConversionScenarios
{
    /// <summary>
    /// Single test scenario data for currency conversion.
    /// </summary>
    public sealed record Scenario(
        string Name,
        Currency Source,
        Domain.ExchangeRates? Snapshot,
        decimal Amount,
        Currency Target,
        HashSet<Currency>? Blacklist,
        Type? ExpectedExceptionType,
        decimal? ExpectedResult);

    /// <summary>
    /// Returns all predefined scenarios to drive parameterized tests.
    /// </summary>
    public static IEnumerable<Func<Scenario>> GetScenarios()
    {
        var currencyFaker = new CurrencyFaker();
        var ratesFaker = new ExchangeRatesFaker();
        var faker = new Faker();

        var sourceCurrency = currencyFaker.Generate();
        var targetCurrency = currencyFaker.Generate();
        var testDate = DateOnly.FromDateTime(faker.Date.Past(2));
        var ratesSnapshot = new Domain.ExchangeRates(
            sourceCurrency,
            testDate,
            new Dictionary<Currency, decimal>
            {
                [targetCurrency] = faker.Random.Decimal(0.5m, 50m)
            });

        yield return () => new Scenario(
            "Null exchangeRates",
            sourceCurrency,
            null,
            faker.Random.Decimal(0.5m, 50m),
            targetCurrency,
            [],
            typeof(ArgumentNullException),
            null);

        yield return () => new Scenario(
            "Null blacklist",
            sourceCurrency,
            ratesSnapshot,
            faker.Random.Decimal(0.5m, 50m),
            targetCurrency,
            null,
            typeof(ArgumentNullException),
            null);

        yield return () => new Scenario(
            "Negative amount",
            sourceCurrency,
            ratesSnapshot,
            faker.Random.Decimal(-50m, -0.5m),
            targetCurrency,
            [],
            typeof(ArgumentOutOfRangeException),
            null);

        yield return () => new Scenario(
            "Source in blacklist",
            sourceCurrency,
            ratesSnapshot,
            faker.Random.Decimal(0.5m, 50m),
            targetCurrency,
            [sourceCurrency],
            typeof(ArgumentException),
            null);

        yield return () => new Scenario(
            "Target in blacklist",
            sourceCurrency,
            ratesSnapshot,
            faker.Random.Decimal(0.5m, 50m),
            targetCurrency,
            [targetCurrency],
            typeof(ArgumentException),
            null);

        // Special snapshot with only one rate, target intentionally missing --
        var snapshotWithMissingRate = new Domain.ExchangeRates(
            sourceCurrency,
            DateOnly.FromDateTime(faker.Date.Past(2)),
            new Dictionary<Currency, decimal>
            {
                // Only one fake rate
                [currencyFaker.Generate()] = faker.Random.Decimal(0.5m, 2.0m)
            }
        );

        // Make sure the target currency is not in the snapshot
        var missingTarget = currencyFaker.Generate();
        while (snapshotWithMissingRate.Rates.ContainsKey(missingTarget) || missingTarget == sourceCurrency)
        {
            missingTarget = currencyFaker.Generate();
        }

        yield return () => new Scenario(
            "Missing target rate",
            sourceCurrency,
            snapshotWithMissingRate,
            faker.Random.Decimal(0.5m, 50m),
            missingTarget,
            [],
            typeof(ArgumentException),
            null);

        var identicalAmount = faker.Random.Decimal(0.5m, 50m);
        yield return () => new Scenario(
            "Identical conversion",
            sourceCurrency,
            ratesSnapshot,
            identicalAmount,
            sourceCurrency,
            [],
            null,
            identicalAmount);

        // Valid conversion scenarios via ExchangeRatesFaker
        var base1 = currencyFaker.Generate();
        var target1 = currencyFaker.Generate();
        var rate1 = faker.Random.Decimal(0.5m, 2.5m);
        var amount1 = faker.Random.Decimal(1m, 1000m);
        yield return () => new Scenario(
            "Valid conversion 1",
            base1,
            new Domain.ExchangeRates(base1, DateOnly.FromDateTime(faker.Date.Past()),
                new Dictionary<Currency, decimal> { [target1] = rate1 }),
            amount1,
            target1,
            [],
            null,
            amount1 * rate1
        );

        var base2 = currencyFaker.Generate();
        var target2 = currencyFaker.Generate();
        var rate2 = faker.Random.Decimal(0.5m, 2.5m);
        var amount2 = faker.Random.Decimal(1m, 1000m);
        yield return () => new Scenario(
            "Valid conversion 2",
            base2,
            new Domain.ExchangeRates(base2, DateOnly.FromDateTime(faker.Date.Past()),
                new Dictionary<Currency, decimal> { [target2] = rate2 }),
            amount2,
            target2,
            [],
            null,
            amount2 * rate2
        );

        var base3 = currencyFaker.Generate();
        var target3 = currencyFaker.Generate();
        var rate3 = faker.Random.Decimal(0.5m, 2.5m);
        var amount3 = faker.Random.Decimal(1m, 1000m);
        yield return () => new Scenario(
            "Valid conversion 3",
            base3,
            new Domain.ExchangeRates(base3, DateOnly.FromDateTime(faker.Date.Past()),
                new Dictionary<Currency, decimal> { [target3] = rate3 }),
            amount3,
            target3,
            [],
            null,
            amount3 * rate3
        );

        var base4 = currencyFaker.Generate();
        var target4 = currencyFaker.Generate();
        var rate4 = faker.Random.Decimal(0.5m, 2.5m);
        var amount4 = faker.Random.Decimal(1m, 1000m);
        yield return () => new Scenario(
            "Valid conversion 4",
            base4,
            new Domain.ExchangeRates(base4, DateOnly.FromDateTime(faker.Date.Past()),
                new Dictionary<Currency, decimal> { [target4] = rate4 }),
            amount4,
            target4,
            [],
            null,
            amount4 * rate4
        );

        var base5 = currencyFaker.Generate();
        var target5 = currencyFaker.Generate();
        var rate5 = faker.Random.Decimal(0.5m, 2.5m);
        var amount5 = faker.Random.Decimal(1m, 1000m);
        yield return () => new Scenario(
            "Valid conversion 5",
            base5,
            new Domain.ExchangeRates(base5, DateOnly.FromDateTime(faker.Date.Past()),
                new Dictionary<Currency, decimal> { [target5] = rate5 }),
            amount5,
            target5,
            [],
            null,
            amount5 * rate5
        );
    }
}
