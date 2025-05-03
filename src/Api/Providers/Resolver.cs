using Microsoft.Extensions.Options;

namespace Api.Providers;

public class Resolver
{
    private readonly Dictionary<string, IExchangeRateProvider> _map;
    private readonly string _defaultProviderName;

    public Resolver(
        IEnumerable<IExchangeRateProvider> providers,
        IOptions<ProvidersOptions> options)
    {
        _map = providers.ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);
        _defaultProviderName = options.Value.FirstOrDefault(kv => kv.Value.IsDefault).Key
                               ?? throw new InvalidOperationException("No default provider is marked as IsDefault.");

        if (!_map.ContainsKey(_defaultProviderName))
        {
            throw new InvalidOperationException(
                $"Default provider '{_defaultProviderName}' is not registered.");
        }
    }

    public IExchangeRateProvider Get(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return _map[_defaultProviderName];
        }

        if (_map.TryGetValue(name, out var provider))
        {
            return provider;
        }

        throw new KeyNotFoundException($"Provider '{name}' is not registered.");
    }
}
