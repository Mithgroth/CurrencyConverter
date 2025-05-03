using Api.Providers;
using Domain;

namespace Api.Features.Currencies;

public static class Endpoints
{
    public static WebApplication MapCurrenciesEndpoints(this WebApplication app)
    {
        app.MapPost("/api/currencies/convert", async (
                ConvertCurrencyRequest request,
                IExchangeRateProvider provider,
                CancellationToken cancellationToken) =>
            {
                try
                {
                    var sourceCurrency = new Currency(request.From);
                    var targetCurrency = new Currency(request.To);

                    var exchangeRates = await provider.GetLatest(sourceCurrency, cancellationToken);
                    var blacklist = new HashSet<Currency>();

                    var convertedAmount = sourceCurrency.Convert(
                        target: targetCurrency,
                        amount: request.Amount,
                        exchangeRates: exchangeRates,
                        blacklist: blacklist);

                    var response = new ConvertCurrencyResponse(
                        From: sourceCurrency.Code,
                        To: targetCurrency.Code,
                        Amount: request.Amount,
                        Rate: exchangeRates.Rates[targetCurrency],
                        ConvertedAmount: convertedAmount);

                    return Results.Ok(response);
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
            .WithName("ConvertCurrency")
            .WithTags("Currencies")
            .Produces<ConvertCurrencyResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

        return app;
    }
}
