using System.Text.Json.Serialization;
using Domain;

namespace Api.Features.Rates;

public sealed record ExchangeRatesResponse
{
    [JsonConstructor]
    public ExchangeRatesResponse(
        string baseCurrency,
        DateOnly date,
        Dictionary<string, decimal> rates)
    {
        BaseCurrency = baseCurrency;
        Date = date;
        Rates = rates;
    }

    public ExchangeRatesResponse(ExchangeRates domain)
        : this(
            domain.BaseCurrency.Code,
            domain.Date,
            domain.Rates.ToDictionary(
                kvp => kvp.Key.Code,
                kvp => kvp.Value))
    {
    }

    public string BaseCurrency { get; }
    public DateOnly Date { get; }
    public Dictionary<string, decimal> Rates { get; }
}
