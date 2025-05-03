using Domain;

namespace Api.Providers;

public interface IExchangeRateProvider
{
    Task<ExchangeRates> GetLatest(Currency baseCurrency, CancellationToken cancellationToken = default);
    Task<List<ExchangeRates>> GetHistorical(Currency baseCurrency, DateOnly from, DateOnly to,
        CancellationToken cancellationToken = default);
}
