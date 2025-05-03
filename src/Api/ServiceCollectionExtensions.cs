using Api.Providers;
using Polly;
using Polly.Extensions.Http;

namespace Api;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Just for easier plumbing.
    /// </summary>
    public static IServiceCollection AddProviderHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("Providers");
        var providers = section.Get<Dictionary<string, ProviderConfig>>() ?? new();

        foreach (var (name, config) in providers)
        {
            services.AddHttpClient(name, client =>
                {
                    client.BaseAddress = new Uri(config.BaseUrl);
                })
                .AddPolicyHandler(GetRetryPolicy())
                .AddPolicyHandler(GetCircuitBreakerPolicy());
        }

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) +
                TimeSpan.FromMilliseconds(Random.Shared.Next(0, 100))
            );
    }

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(5)
            );
    }
}
