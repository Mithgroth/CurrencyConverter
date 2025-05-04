using System.Net;
using System.Net.Http.Json;

namespace Integration;

public class Resolver : TestBase
{
    [Test]
    public async Task FailsForUnknownProvider()
    {
        // Arrange
        var client = WebApplicationFactory.CreateClient();
        var request = new
        {
            From = "USD",
            To = "EUR",
            Amount = 100.0
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/currencies/conversion")
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
