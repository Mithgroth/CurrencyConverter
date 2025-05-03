using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Integration;

public class Resolver
{
    [Test]
    public async Task FailsForUnknownProvider()
    {
        // Arrange
        var app = new WebApplicationFactory<Program>();
        var client = app.CreateClient();
        var request = new
        {
            From = "USD",
            To = "EUR",
            Amount = 100.0
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/currencies/convert")
        {
            Content = JsonContent.Create(request)
        };
        httpRequest.Headers.Add("X-Exchange-Provider", "theproviderthatneverexisted");

        // Act
        var response = await client.SendAsync(httpRequest);
        var body = await response.Content.ReadAsStringAsync();

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        await Assert.That(body).Contains("Unknown exchange rate provider");
        await Assert.That(body).Contains("theproviderthatneverexisted");
    }
}
