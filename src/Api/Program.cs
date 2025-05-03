using Api.Features.Rates;
using Api.Providers;
using Frankfurter = Api.Providers.Frankfurter;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddOpenApi();
services.AddHttpClient("Frankfurter", client =>
{
    client.BaseAddress = new Uri("https://api.frankfurter.dev");
});

services.AddScoped<IExchangeRateProvider, Frankfurter.Provider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapRatesEndpoints();

app.Run();

public partial class Program; // Only used for testing in Integration.WebApplicationFactory
