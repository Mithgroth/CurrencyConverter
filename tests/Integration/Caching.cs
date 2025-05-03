using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Integration;

public class Caching
{
    [Test]
    public async Task CanHit()
    {
        // Arrange
        var cache = new TrackingMemoryCache();

        var app = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton<IMemoryCache>(_ => cache); // override real cache
                });
            });

        var client = app.CreateClient();

        // Act
        var res1 = await client.GetAsync("/api/v1/rates");
        var res2 = await client.GetAsync("/api/v1/rates");
        var res3 = await client.GetAsync("/api/v1/rates");

        // Assert
        await Assert.That(res1.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(res2.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(res3.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(cache.Misses).IsEqualTo(1);
        await Assert.That(cache.Hits).IsEqualTo(2);
    }
}
