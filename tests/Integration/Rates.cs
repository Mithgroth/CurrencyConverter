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
        var response = await client.GetAsync("/api/v1/rates"); // TODO: Add query parameters

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

    [Test]
    public async Task GetHistorical()
    {
        // Arrange
        var client = WebApplicationFactory.CreateClient();

        var from = new DateOnly(2020, 1, 2);
        var to = new DateOnly(2020, 1, 31);
        const string baseCurrency = "EUR";
        const int page = 1;
        const int pageSize = 5;

        var url = $"/api/v1/rates/historical" +
                  $"?base={baseCurrency}" +
                  $"&from={from:yyyy-MM-dd}" +
                  $"&to={to:yyyy-MM-dd}" +
                  $"&page={page}" +
                  $"&pageSize={pageSize}";

        var expectedDates = GetBusinessDays(from, to);

        // Act
        var response = await client.GetAsync(url);
        var json = await response.Content.ReadAsStringAsync();
        var parsed = JsonSerializer.Deserialize<HistoricalRatesResponse>(json, JsonSerializerOptions.Web)!;

        // Assert
        await Assert.That(parsed).IsNotNull();
        await Assert.That(parsed.BaseCurrency).IsEqualTo(baseCurrency);
        await Assert.That(parsed.Rates).IsNotEmpty();

        // Ensure each expected business date is present in the returned Rates
        foreach (var expectedDate in expectedDates)
        {
            var key = expectedDate.ToString("yyyy-MM-dd");

            await Assert.That(parsed.Rates).ContainsKey(key);

            var rateMap = parsed.Rates[key];
            await Assert.That(rateMap).IsNotEmpty();
            await Assert.That(rateMap).ContainsKey("USD");
            await Assert.That(rateMap).ContainsKey("GBP");
            await Assert.That(rateMap).ContainsKey("JPY");
        }
    }

    private HashSet<DateOnly> GetBusinessDays(DateOnly from, DateOnly to)
    {
        var days = new HashSet<DateOnly>();
        for (var d = from; d <= to; d = d.AddDays(1))
        {
            if (d.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday)
            {
                days.Add(d);
            }
        }

        return days;
    }
}
