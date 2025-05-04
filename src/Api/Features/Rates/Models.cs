using System.ComponentModel.DataAnnotations;
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

public sealed record HistoricalRatesRequest
{
    [Required]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency code must be 3 letters.")]
    [RegularExpression("^[a-zA-Z]{3}$", ErrorMessage = "Currency code must contain only letters.")]
    public string BaseCurrency { get; init; } = "EUR";

    [Required]
    public DateOnly? From { get; init; }

    [Required]
    public DateOnly? To { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "Page must be 1 or greater.")]
    public int Page { get; init; } = 1;

    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
    public int PageSize { get; init; } = 10;
}


public sealed record HistoricalRatesResponse
{
    [JsonConstructor]
    public HistoricalRatesResponse(
        string baseCurrency,
        DateOnly startDate,
        DateOnly endDate,
        Dictionary<string, Dictionary<string, decimal>> rates)
    {
        BaseCurrency = baseCurrency;
        StartDate = startDate;
        EndDate = endDate;
        Rates = rates;
    }

    public HistoricalRatesResponse(List<ExchangeRates> domain)
    {
        if (domain.Count == 0)
        {
            throw new ArgumentException("Historical data must contain at least one item.", nameof(domain));
        }

        BaseCurrency = domain[0].BaseCurrency.Code;
        StartDate = domain.Min(x => x.Date);
        EndDate = domain.Max(x => x.Date);

        Rates = domain.ToDictionary(
            x => x.Date.ToString("yyyy-MM-dd"),
            x => x.Rates.ToDictionary(kvp => kvp.Key.Code, kvp => kvp.Value)
        );
    }

    public string BaseCurrency { get; }
    public DateOnly StartDate { get; }
    public DateOnly EndDate { get; }
    public Dictionary<string, Dictionary<string, decimal>> Rates { get; }
}
