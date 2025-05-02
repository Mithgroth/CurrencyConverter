using Domain;
using TUnit.Assertions.AssertConditions.Throws;

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
}
