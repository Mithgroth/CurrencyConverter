using Api.Features.Currencies;
using Api.Features.Rates;
using Api.Providers;
using Polly;
using Polly.Extensions.Http;
using Frankfurter = Api.Providers.Frankfurter;
using Resolver = Api.Providers.Resolver;
using Rates = Api.Features.Rates;
using Currencies = Api.Features.Currencies;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddOpenApi();
services.AddMemoryCache();

services.AddScoped<Resolver>();
services
    .AddHttpClient("Frankfurter", client => { client.BaseAddress = new Uri("https://api.frankfurter.dev"); })
    .AddPolicyHandler(HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, retryAttempt =>
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) +
            TimeSpan.FromMilliseconds(Random.Shared.Next(0, 100))
        ))
    .AddPolicyHandler(HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5,
            durationOfBreak: TimeSpan.FromSeconds(5)
        ));

services.AddScoped<IExchangeRateProvider, Frankfurter.Provider>();
services.AddScoped<Rates.Service>();
services.AddScoped<Currencies.Service>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapRatesEndpoints();
app.MapCurrenciesEndpoints();

app.Run();

public partial class Program; // Only used for testing in Integration.WebApplicationFactory
