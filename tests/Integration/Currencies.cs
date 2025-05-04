using System.Net;
using System.Net.Http.Json;
using Api.Features.Currencies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Integration;

public class Currencies
{
    [ClassDataSource<WebApplicationFactory>(Shared = SharedType.PerTestSession)]
    public required WebApplicationFactory WebApplicationFactory { get; init; }

    [Test]
    public async Task ConvertCurrency()
    {
        // Arrange
        var client = WebApplicationFactory.CreateClient();

        var request = new ConvertCurrencyRequest
        {
            From = "EUR",
            To = "USD",
            Amount = 100m
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/currencies/convert", request);
        var result = await response.Content.ReadFromJsonAsync<ConvertCurrencyResponse>();

        // Assert
        await Assert.That(response.IsSuccessStatusCode).IsTrue();
        await Assert.That(result).IsNotNull();
        await Assert.That(result!.From).IsEqualTo(request.From);
        await Assert.That(result.To).IsEqualTo(request.To);
        await Assert.That(result.Amount).IsEqualTo(request.Amount);
        await Assert.That(result.Rate).IsGreaterThan(0);
        await Assert.That(result.ConvertedAmount).IsEqualTo(result.Amount * result.Rate).Within(0.01m);
    }

    [Test]
    public async Task Blacklist()
    {
        // Arrange
        var client = WebApplicationFactory.CreateClient();
        var config = WebApplicationFactory.Services.GetRequiredService<IConfiguration>();
        var blacklisted = config.GetSection("CurrencyOptions:BlacklistedCurrencies").Get<List<string>>()!;

        foreach (var currency in blacklisted)
        {
            var requestFrom = new ConvertCurrencyRequest
            {
                From = currency,
                To = "USD",
                Amount = 100m
            };

            var requestTo = new ConvertCurrencyRequest
            {
                From = "USD",
                To = currency,
                Amount = 100m
            };

            // Act
            var responseFrom = await client.PostAsJsonAsync("/api/v1/currencies/convert", requestFrom);
            var responseTo = await client.PostAsJsonAsync("/api/v1/currencies/convert", requestTo);

            // Assert
            await Assert.That(responseFrom.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
            await Assert.That(responseTo.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        }
    }

}
