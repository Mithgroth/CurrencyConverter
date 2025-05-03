using Domain;

namespace Api.Providers;

public interface IExchangeRateProvider
{
    Task<ExchangeRates> GetLatest(Currency baseCurrency, CancellationToken cancellationToken = default);
}
