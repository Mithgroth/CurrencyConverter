using Domain;

namespace Api.Providers;

public interface IExchangeRateProvider
{
    /// <summary>
    /// The key clients will use in the header, e.g. "Frankfurter"
    /// </summary>
    string Name { get; }

    Task<ExchangeRates> GetLatest(Currency baseCurrency, CancellationToken cancellationToken = default);
    Task<List<ExchangeRates>> GetHistorical(Currency baseCurrency, DateOnly from, DateOnly to,
        CancellationToken cancellationToken = default);
}
