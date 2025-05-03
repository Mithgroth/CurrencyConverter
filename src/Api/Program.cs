using Api;
using Api.Features.Currencies;
using Api.Features.Rates;
using Api.Providers;
using Microsoft.AspNetCore.Mvc;
using Frankfurter = Api.Providers.Frankfurter;
using Resolver = Api.Providers.Resolver;
using Rates = Api.Features.Rates;
using Currencies = Api.Features.Currencies;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

configuration
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

services.Configure<ProvidersOptions>(configuration.GetSection("Providers"));

services.AddOpenApi();
services.AddMemoryCache();
services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

services.AddScoped<Resolver>();
services.AddProviderHttpClients(configuration);
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
