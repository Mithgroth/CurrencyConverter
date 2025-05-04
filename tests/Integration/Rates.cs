using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Api;
using Api.Features.Rates;
using Microsoft.AspNetCore.Mvc;

namespace Integration;

public class Rates : TestBase
{
    [Test]
    public async Task GetLatest()
    {
        // Arrange
        var client = WebApplicationFactory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/rates"); // TODO: Add query parameters

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Status: {response.StatusCode}, Body: {body}");
            Assert.Fail("Request did not succeed.");
        }

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
                  $"?baseCurrency={baseCurrency}" +
                  $"&from={from:yyyy-MM-dd}" +
                  $"&to={to:yyyy-MM-dd}" +
                  $"&page={page}" +
                  $"&pageSize={pageSize}";

        var expectedDates = GetBusinessDays(from, to)
            .OrderBy(d => d)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Act
        var response = await client.GetAsync(url);
        var json = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new DateOnlyJsonConverter());
        var parsed = JsonSerializer.Deserialize<HistoricalRatesResponse>(json, options)!;

        // Assert
        await Assert.That(response.IsSuccessStatusCode).IsTrue();
        await Assert.That(parsed).IsNotNull();
        await Assert.That(parsed.BaseCurrency).IsEqualTo(baseCurrency);
        await Assert.That(parsed.Rates).IsNotEmpty();

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

    [Test]
    public async Task HistoricalPagination()
    {
        // Arrange
        var client = WebApplicationFactory.CreateClient();

        var requestParams = new Dictionary<string, string>
        {
            ["baseCurrency"] = "EUR",
            ["from"] = "2025-04-06",
            ["to"] = "2025-04-20",
            ["page"] = "2",
            ["pageSize"] = "3"
        };

        var query = string.Join("&", requestParams.Select(kv => $"{kv.Key}={kv.Value}"));
        var url = $"/api/v1/rates/historical?{query}";

        // Act
        var response = await client.GetAsync(url);
        var json = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new DateOnlyJsonConverter());
        var parsed = JsonSerializer.Deserialize<HistoricalRatesResponse>(json, options)!;

        // Assert
        await Assert.That(response.IsSuccessStatusCode).IsTrue();
        await Assert.That(parsed).IsNotNull();
        await Assert.That(parsed!.Rates.Count).IsEqualTo(3);

        var sortedDates = parsed.Rates
            .Keys
            .Select(DateOnly.Parse)
            .OrderBy(d => d)
            .ToList();


        await Assert.That(sortedDates.Count).IsEqualTo(3);

        foreach (var date in sortedDates)
        {
            await Assert.That(date.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday).IsTrue();
        }

        await Assert.That(sortedDates.First()).IsEqualTo(new DateOnly(2025, 4, 9));
        await Assert.That(sortedDates.Last()).IsEqualTo(new DateOnly(2025, 4, 11));
    }

    // Validation tests
    [Test]
    public async Task GetLatest_InvalidBaseCurrency_ReturnsBadRequest()
    {
        // Arrange
        var client = WebApplicationFactory.CreateClient();
        var url = "/api/v1/rates?baseCurrency=E"; // Invalid: too short

        // Act
        var response = await client.GetAsync(url);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        await Assert.That(problem).IsNotNull();
        await Assert.That(problem!.Errors["BaseCurrency"]).IsNotEmpty();
    }

    [Test]
    public async Task GetHistorical_MissingToDate_ReturnsBadRequest()
    {
        // Arrange
        var client = WebApplicationFactory.CreateClient();
        var url = "/api/v1/rates/historical?base=EUR&from=2020-01-01&page=1&pageSize=5"; // Missing "to" date

        // Act
        var response = await client.GetAsync(url);
        var contentType = response.Content.Headers.ContentType?.MediaType;
        var body = await response.Content.ReadAsStringAsync();

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        await Assert.That(body).IsNotEmpty();

        if (contentType == "application/json")
        {
            var problem = JsonSerializer.Deserialize<ValidationProblemDetails>(body,
                new JsonSerializerOptions(JsonSerializerDefaults.Web))!;

            await Assert.That(problem.Errors.Keys).Contains("To");
            await Assert.That(problem.Errors["To"]).IsNotEmpty();
        }
        else
        {
            // Fallback: plain string body with error message
            await Assert.That(body).Contains("To", StringComparison.OrdinalIgnoreCase);
        }
    }
}
