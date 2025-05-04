using System.Net;
using Integration.Utilities;

namespace Integration;

public class Throttling
{
    [ClassDataSource<ThrottlingWebApplicationFactory>(Shared = SharedType.PerTestSession)]
    public required ThrottlingWebApplicationFactory WebApplicationFactory { get; init; }

    [Test]
    public async Task CanLimitWith429()
    {
        // Arrange
        var client = WebApplicationFactory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", "Intern");

        var successes = 0;
        var rateLimited = 0;

        // Act
        for (var i = 0; i < 15; i++)
        {
            var response = await client.GetAsync("/api/v1/rates");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                successes++;
            }
            else if (response.StatusCode == (HttpStatusCode)429)
            {
                rateLimited++;
            }
            else
            {
                throw new Exception($"Unexpected status: {response.StatusCode}");
            }
        }

        // Assert
        await Assert.That(successes).IsGreaterThan(0);
        await Assert.That(rateLimited).IsGreaterThan(0);
    }
}
