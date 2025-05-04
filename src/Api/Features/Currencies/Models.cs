using System.ComponentModel.DataAnnotations;

namespace Api.Features.Currencies;

public sealed record CurrencyOptions
{
    public List<string> BlacklistedCurrencies { get; init; } = new();
}

public sealed record ConvertCurrencyRequest
{
    [Required]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "From currency must be 3 letters.")]
    [RegularExpression("^[a-zA-Z]{3}$", ErrorMessage = "Currency code must contain only letters.")]
    public string From { get; init; } = string.Empty;

    [Required]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "To currency must be 3 letters.")]
    [RegularExpression("^[a-zA-Z]{3}$", ErrorMessage = "Currency code must contain only letters.")]
    public string To { get; init; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
    public decimal Amount { get; init; }
}


public sealed record ConvertCurrencyResponse(
    string From,
    string To,
    decimal Amount,
    decimal Rate,
    decimal ConvertedAmount);
