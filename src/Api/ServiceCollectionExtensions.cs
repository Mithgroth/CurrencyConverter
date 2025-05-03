using Api.Providers;
using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using Polly.Retry;

namespace Api;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Just for easier plumbing.
    /// </summary>
    public static IServiceCollection AddProviderHttpClients(this IServiceCollection services,
        IConfiguration configuration)
    {
        var section = configuration.GetSection("Providers");
        var providers = section.Get<Dictionary<string, ProviderConfig>>() ?? new Dictionary<string, ProviderConfig>();

        foreach (var (name, config) in providers)
        {
            services.AddHttpClient(name, client => { client.BaseAddress = new Uri(config.BaseUrl); })
                .AddHttpMessageHandler<CorrelationIdHandler>()
                .AddPolicyHandler(GetRetryPolicy())
                .AddPolicyHandler(GetCircuitBreakerPolicy());
        }

        return services;
    }

    private static AsyncRetryPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) +
                TimeSpan.FromMilliseconds(Random.Shared.Next(0, 100))
            );
    }

    private static AsyncCircuitBreakerPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(5)
            );
    }
}
