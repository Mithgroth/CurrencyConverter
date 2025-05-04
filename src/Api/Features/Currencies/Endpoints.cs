using Microsoft.Extensions.Options;

namespace Api.Features.Currencies;

public static class Endpoints
{
    public static WebApplication MapCurrenciesEndpoints(this WebApplication app)
    {
        var v1 = app.MapGroup("/api/v1");

        v1.MapPost("/currencies/convert", async (
                HttpRequest httpRequest,
                ConvertCurrencyRequest request,
                Service service,
                IOptions<CurrencyOptions> options,
                CancellationToken cancellationToken) =>
            {
                var providerName = httpRequest.GetProviderName();

                try
                {
                    var response = await service.ConvertCurrency(request, providerName, options, cancellationToken);
                    return Results.Ok(response);
                }
                catch (KeyNotFoundException)
                {
                    return Results.BadRequest($"Unknown exchange rate provider: '{providerName}'");
                }
                catch (ArgumentException ex)
                {
                    return Results.Problem(
                        title: "Invalid Currency Conversion Request",
                        detail: ex.Message,
                        statusCode: StatusCodes.Status400BadRequest);
                }
                catch (Exception ex)
                {
                    return Results.Problem(
                        title: "Unexpected Error",
                        detail: ex.Message,
                        statusCode: StatusCodes.Status500InternalServerError);
                }
            })
            .RequireAuthorization(policy => policy.RequireRole("FinancialExpert"))
            .WithName("ConvertCurrency")
            .WithTags("Currencies")
            .Produces<ConvertCurrencyResponse>(StatusCodes.Status200OK)
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
