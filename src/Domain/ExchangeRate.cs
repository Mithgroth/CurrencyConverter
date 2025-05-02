namespace Domain;

public sealed record ExchangeRate
{
    public Currency BaseCurrency { get; }
    public DateOnly Date { get; }
    public IReadOnlyDictionary<Currency, decimal> Rates { get; }

    public ExchangeRate(Currency baseCurrency, DateOnly date, IReadOnlyDictionary<Currency, decimal> rates)
    {
        if (rates is null || rates.Count == 0)
        {
            throw new ArgumentException("Rates must not be empty", nameof(rates));
        }

        BaseCurrency = baseCurrency;
        Date = date;
        Rates = rates;
    }
}
