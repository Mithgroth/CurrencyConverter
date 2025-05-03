namespace Api.Providers;

public class Resolver
{
    private readonly Dictionary<string, IExchangeRateProvider> _map;

    public Resolver(IEnumerable<IExchangeRateProvider> providers)
    {
        _map = providers.ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);
    }

    public IExchangeRateProvider Get(string name)
    {
        if (_map.TryGetValue(name.ToLower(), out var provider))
        {
            return provider;
        }

        throw new KeyNotFoundException($"Provider '{name}' is not registered.");
    }
}
