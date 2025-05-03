using Api.Providers;
using Domain;

namespace Api.Features.Rates;

public static class Endpoints
{
    public static WebApplication MapRatesEndpoints(this WebApplication app)
    {
        app.MapGet("/api/rates", async (
                [AsParameters] ExchangeRatesRequest request,
                IExchangeRateProvider provider,
                CancellationToken cancellationToken
            ) =>
            {
                try
                {
                    var result = await provider.GetLatest(new Currency(request.BaseCurrency), cancellationToken);
                    return Results.Ok(new ExchangeRatesResponse(result));
                }
                catch (ArgumentException ex)
                {
                    return Results.Problem(title: "Argument Exception", detail: ex.Message);
                }
            })
            .WithName("GetLatestRates")
            .WithTags("Rates")
            .Produces<ExchangeRates>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        app.MapGet("/api/rates/historical", async (
                [AsParameters] HistoricalRatesRequest request,
                IExchangeRateProvider provider,
                CancellationToken cancellationToken) =>
            {
                try
                {
                    var result = await provider.GetHistorical(
                        new Currency(request.Base),
                        request.From,
                        request.To,
                        cancellationToken);

                    return Results.Ok(new HistoricalRatesResponse(result));
                }
                catch (ArgumentException ex)
                {
                    return Results.Problem(title: "Argument Exception", detail: ex.Message);
                }
            })
            .WithName("GetHistoricalRates")
            .WithTags("Rates")
            .Produces<HistoricalRatesResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        return app;
    }
}
