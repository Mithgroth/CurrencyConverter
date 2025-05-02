using Bogus;
using Domain;
using TUnit.Assertions.AssertConditions.Throws;
using Unit.Fakers;

namespace Unit;

public class ExchangeRates
{
    [Repeat(4)]
    [Test]
    public async Task CanBeCreated()
    {
        // Arrange
        var faker = new Faker();
        var currencyFaker = new CurrencyFaker();

        var baseCurrency = currencyFaker.Generate();
        var date = DateOnly.FromDateTime(faker.Date.Recent(30));
        var currency1 = currencyFaker.Generate();
        var currency2 = currencyFaker.Generate();
        var currency3 = currencyFaker.Generate();

        var rate1 = faker.Finance.Amount(0.5m, 50m);
        var rate2 = faker.Finance.Amount(0.5m, 50m);
        var rate3 = faker.Finance.Amount(0.5m, 50m);

        var rates = new Dictionary<Currency, decimal>
        {
            [currency1] = rate1,
            [currency2] = rate2,
            [currency3] = rate3
        };

        // Act
        var exchangeRate = new ExchangeRate(baseCurrency, date, rates);

        // Assert
        await Assert.That(exchangeRate.BaseCurrency).IsEqualTo(baseCurrency);
        await Assert.That(exchangeRate.Date).IsEqualTo(date);
        await Assert.That(exchangeRate.Rates.Count).IsEqualTo(3);
        await Assert.That(exchangeRate.Rates[currency1]).IsEqualTo(rate1);
        await Assert.That(exchangeRate.Rates[currency2]).IsEqualTo(rate2);
        await Assert.That(exchangeRate.Rates[currency3]).IsEqualTo(rate3);
    }

    [Test]
    public async Task ThrowsOnEmptyRawRates()
    {
        await Assert.That(() =>
                new ExchangeRate(
                    new Currency("EUR"),
                    DateOnly.FromDateTime(DateTime.Today),
                    new Dictionary<Currency, decimal>())
            )
            .Throws<ArgumentException>();
    }
}
