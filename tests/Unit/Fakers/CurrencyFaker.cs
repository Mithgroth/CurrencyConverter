using Bogus;
using Domain;

namespace Unit.Fakers;

public sealed class CurrencyFaker : Faker<Currency>
{
    private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public CurrencyFaker()
    {
        CustomInstantiator(f =>
        {
            var code = f.Random.String2(3, Chars);
            return new Currency(code);
        });
    }
}
