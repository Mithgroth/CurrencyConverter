using Domain;
using TUnit.Assertions.AssertConditions.Throws;
using Unit.Formatters;
using Unit.Scenarios;

namespace Unit;

public class Currencies
{
    [Test]
    [Arguments("ABC")]
    [Arguments("usd")]
    [Arguments("EUR")]
    [Arguments("try")]
    [Arguments("GBP")]
    public async Task CanBeCreated(string code)
    {
        var currency = new Currency(code);

        await Assert.That(currency.Code).IsEqualTo(code.ToUpperInvariant());
    }

    [Test]
    [Arguments("")]
    [Arguments(" ")]
    [Arguments(null)]
    [Arguments("USDD")]
    [Arguments("1AB")]
    public async Task ThrowsOnInvalidCode(string? code)
    {
        await Assert.That(() => new Currency(code!))
            .Throws<ArgumentException>();
    }

    // Currency Conversion
    [Test]
    [MethodDataSource(typeof(CurrencyConversionScenarios), nameof(CurrencyConversionScenarios.GetScenarios))]
    [ArgumentDisplayFormatter<CurrencyConversionFormatter>]
    public async Task CanBeConverted(CurrencyConversionScenarios.Scenario scenario)
    {
        var source = scenario.Source;
        var snapshot = scenario.Snapshot!;
        var amount = scenario.Amount;
        var target = scenario.Target;
        var blacklist = scenario.Blacklist!;

        if (scenario.ExpectedExceptionType is not null)
        {
            var act = () => Task.FromResult(source.Convert(target, amount, snapshot, blacklist));
            var ex = await Assert.ThrowsAsync<Exception>(act);
            await Assert.That(ex.GetType()).IsEqualTo(scenario.ExpectedExceptionType);
        }
        else
        {
            var result = source.Convert(target, amount, snapshot, blacklist);
            var expected = scenario.ExpectedResult!.Value;

            await Assert.That(Math.Abs(result - expected) < 0.0001m).IsTrue();
        }
    }
}
