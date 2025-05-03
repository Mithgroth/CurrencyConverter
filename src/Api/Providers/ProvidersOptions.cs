namespace Api.Providers;

public class ProviderConfig
{
    public required string BaseUrl { get; set; }
    public bool IsDefault { get; set; } = false;
}

public class ProvidersOptions : Dictionary<string, ProviderConfig> { }
