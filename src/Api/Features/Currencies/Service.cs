using Api.Providers;
using Domain;

namespace Api.Features.Currencies;

public class Service(Resolver resolver)
{
    public async Task<ConvertCurrencyResponse> ConvertCurrency(
        ConvertCurrencyRequest request,
        string providerName,
        CancellationToken cancellationToken)
    {
        var provider = resolver.Get(providerName);

        var sourceCurrency = new Currency(request.From);
        var targetCurrency = new Currency(request.To);

        var exchangeRates = await provider.GetLatest(sourceCurrency, cancellationToken);
        var blacklist = new HashSet<Currency>();

        var convertedAmount = sourceCurrency.Convert(
            target: targetCurrency,
            amount: request.Amount,
            exchangeRates: exchangeRates,
            blacklist: blacklist);

        return new ConvertCurrencyResponse(
            From: sourceCurrency.Code,
            To: targetCurrency.Code,
            Amount: request.Amount,
            Rate: exchangeRates.Rates[targetCurrency],
            ConvertedAmount: convertedAmount);
    }
}
