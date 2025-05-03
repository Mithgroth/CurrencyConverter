namespace Domain;

public sealed record Currency
{
    public string Code { get; }

    public Currency(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Currency code is required.", nameof(code));
        }

        code = code.Trim().ToUpperInvariant();
        if (code.Length != 3 || !code.All(char.IsLetter))
        {
            throw new ArgumentException($"Invalid currency code: '{code}'", nameof(code));
        }

        Code = code;
    }

    public override string ToString() => Code;

    public decimal Convert(
        Currency target,
        decimal amount,
        ExchangeRates exchangeRates,
        IReadOnlySet<Currency>? blacklist)
    {
        ValidateParameters();

        if (Code == target.Code)
        {
            return amount;
        }

        var rate = exchangeRates.Rates[target];
        return amount * rate;

        void ValidateParameters()
        {
            ArgumentNullException.ThrowIfNull(exchangeRates);
            ArgumentNullException.ThrowIfNull(blacklist);

            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be non-negative.");
            }

            if (blacklist.Contains(this))
            {
                throw new ArgumentException($"Source currency '{Code}' is not supported.");
            }

            if (blacklist.Contains(target))
            {
                throw new ArgumentException($"Target currency '{target.Code}' is not supported.", nameof(target));
            }

            if (target.Code != exchangeRates.BaseCurrency.Code && !exchangeRates.Rates.ContainsKey(target))
            {
                throw new ArgumentException($"Currency '{target.Code}' is not available in the provided rates.",
                    nameof(target));
            }
        }
    }
}
