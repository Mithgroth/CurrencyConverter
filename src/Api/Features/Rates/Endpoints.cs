using Domain;

namespace Api.Features.Rates;

public static class Endpoints
{
    public static WebApplication MapRatesEndpoints(this WebApplication app)
    {
        var v1 = app.MapGroup("/api/v1");

        v1.MapGet("/rates", async (
                HttpRequest httpRequest,
                [AsParameters] ExchangeRatesRequest request,
                Service service,
                CancellationToken cancellationToken) =>
            {
                var providerName = httpRequest.GetProviderName();

                try
                {
                    var result = await service.GetLatest(request.BaseCurrency, providerName, cancellationToken);
                    return Results.Ok(new ExchangeRatesResponse(result));
                }
                catch (KeyNotFoundException)
                {
                    return Results.BadRequest($"Unknown exchange rate provider: '{providerName}'");
                }
                catch (ArgumentException ex)
                {
                    return Results.Problem(title: "Argument Exception", detail: ex.Message);
                }
            })
            .RequireAuthorization(policy => policy.RequireRole("Intern", "FinancialExpert"))
            .WithName("GetLatestRates")
            .WithTags("Rates")
            .Produces<ExchangeRates>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        v1.MapGet("/rates/historical", async (
                HttpRequest httpRequest,
                [AsParameters] HistoricalRatesRequest request,
                Service service,
                CancellationToken cancellationToken) =>
            {
                var providerName = httpRequest.GetProviderName();

                try
                {
                    var result = await service.GetHistorical(request.Base, request.From, request.To, providerName, cancellationToken);
                    return Results.Ok(new HistoricalRatesResponse(result));
                }
                catch (KeyNotFoundException)
                {
                    return Results.BadRequest($"Unknown exchange rate provider: '{providerName}'");
                }
                catch (ArgumentException ex)
                {
                    return Results.Problem(title: "Argument Exception", detail: ex.Message);
                }
            })
            .RequireAuthorization(policy => policy.RequireRole("FinancialExpert"))
            .WithName("GetHistoricalRates")
            .WithTags("Rates")
            .Produces<HistoricalRatesResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        return app;
    }

    private static string GetProviderName(this HttpRequest httpRequest)
    {
        return httpRequest.Headers.TryGetValue("X-Exchange-Provider", out var header)
            ? header.ToString()
            : "frankfurter";
    }
}
