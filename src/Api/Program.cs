using System.Text;
using System.Threading.RateLimiting;
using Api;
using Api.Features.Auth;
using Api.Features.Currencies;
using Api.Features.Rates;
using Api.Middlewares;
using Api.Providers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Enrichers.Span;
using Auth = Api.Features.Auth;
using Currencies = Api.Features.Currencies;
using Frankfurter = Api.Providers.Frankfurter;
using Rates = Api.Features.Rates;
using Resolver = Api.Providers.Resolver;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .Enrich.WithProperty("Service", "CurrencyConverter.API")
    .Enrich.WithSpan()
    //.WriteTo.Seq("<Seq URL>")
    .CreateLogger();

builder.Host.UseSerilog();

configuration
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddConsoleExporter() // Grafana in production?
            .SetResourceBuilder(
                ResourceBuilder.CreateDefault()
                    .AddService("CurrencyConverter.API"));
    });
services.Configure<ProvidersOptions>(configuration.GetSection("Providers"));
services.Configure<CurrencyOptions>(configuration.GetSection("CurrencyOptions"));

services.AddOpenApi();
services.AddMemoryCache();
services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});
services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new DateOnlyJsonConverter());
});

services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 10,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsync("Rate limit exceeded", token);
    };
});

services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var issuer = configuration["Jwt:Issuer"]
                     ?? throw new InvalidOperationException("JWT Issuer is not configured.");
        var audience = configuration["Jwt:Audience"]
                       ?? throw new InvalidOperationException("JWT Audience is not configured.");
        var key = configuration["Jwt:Key"]
                  ?? throw new InvalidOperationException("JWT Key is not configured.");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
    });
services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy =>
        policy.RequireRole("Admin"));
});


services.AddHttpContextAccessor();
services.AddTransient<CorrelationIdHandler>();

services.AddProviderHttpClients(configuration);
services.AddScoped<Resolver>();
services.AddScoped<IExchangeRateProvider, Frankfurter.Provider>();

services.AddScoped<Auth.Service>();
services.AddScoped<Currencies.Service>();
services.AddScoped<Rates.Service>();

var app = builder.Build();

app.UseMiddleware<CorrelationId>();
app.UseMiddleware<RequestLogging>();
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapCurrenciesEndpoints();
app.MapRatesEndpoints();

app.Run();

public partial class Program; // Only used for testing in Integration.WebApplicationFactory
