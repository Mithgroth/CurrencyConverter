using Api.Providers;
using Domain;
using Microsoft.AspNetCore.Mvc;

namespace Api.Features.Rates;

public static class Endpoints
{
    public static WebApplication MapRatesEndpoints(this WebApplication app)
    {
        app.MapGet("/api/rates", async (
                IExchangeRateProvider provider,
                CancellationToken cancellationToken,
                [FromQuery] string baseCurrency = "EUR") =>
            {
                try
                {
                    var result = await provider.GetLatest(new Currency(baseCurrency), cancellationToken);
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

        return app;
    }
}
