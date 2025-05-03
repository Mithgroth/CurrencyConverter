using System.Text.Json;
using Api.Features.Rates;

namespace Integration;

public class Rates
{
    [ClassDataSource<WebApplicationFactory>(Shared = SharedType.PerTestSession)]
    public required WebApplicationFactory WebApplicationFactory { get; init; }

    [Test]
    public async Task GetLatest()
    {
        // Arrange
        var client = WebApplicationFactory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/rates");

        var json = await response.Content.ReadAsStringAsync();
        var parsed = JsonSerializer.Deserialize<ExchangeRatesResponse>(json, JsonSerializerOptions.Web)!;

        // Assert
        await Assert.That(response.IsSuccessStatusCode).IsTrue();
        await Assert.That(parsed).IsNotNull();
        await Assert.That(parsed.BaseCurrency).IsEqualTo("EUR");
        await Assert.That(parsed.Date).IsEqualTo(GetExpectedLatestDate());
        await Assert.That(parsed.Rates).IsNotEmpty();
        await Assert.That(parsed.Rates).ContainsKey("USD");
        await Assert.That(parsed.Rates).ContainsKey("GBP");
        await Assert.That(parsed.Rates).ContainsKey("JPY");

        // Rates only change on business days
        DateOnly GetExpectedLatestDate()
        {
            var today = DateTime.UtcNow.Date;
            return DateOnly.FromDateTime(today.DayOfWeek switch
            {
                DayOfWeek.Saturday => today.AddDays(-1),
                DayOfWeek.Sunday => today.AddDays(-2),
                _ => today
            });
        }
    }
}
