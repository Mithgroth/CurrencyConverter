using System.ComponentModel.DataAnnotations;

namespace Api.Features.Currencies;

public sealed record CurrencyOptions
{
    public List<string> BlacklistedCurrencies { get; init; } = new();
}

public sealed record ConvertCurrencyRequest
{
    [Required] public string From { get; init; } = string.Empty;

    [Required] public string To { get; init; } = string.Empty;

    [Range(0.01, double.MaxValue)] public decimal Amount { get; init; }
}

public sealed record ConvertCurrencyResponse(
    string From,
    string To,
    decimal Amount,
    decimal Rate,
    decimal ConvertedAmount);
