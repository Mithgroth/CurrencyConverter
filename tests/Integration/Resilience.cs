using Integration.TestHandlers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Polly.CircuitBreaker;

namespace Integration;

public class Resilience
{
    private readonly AlwaysFail _alwaysFailHandler = new();
    private readonly TwoFailOneSuccess _twoFailOneSuccessHandler = new();

    [ClassDataSource<WebApplicationFactory<Program>>(Shared = SharedType.PerTestSession)]
    public required WebApplicationFactory<Program> Factory { get; init; }

    [Test]
    public async Task CanRetry()
    {
        // Arrange
        var factory = Factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<DelegatingHandler>(_twoFailOneSuccessHandler);
                services.PostConfigure<HttpClientFactoryOptions>("Frankfurter",
                    options =>
                    {
                        options.HttpMessageHandlerBuilderActions.Add(handlerBuilder =>
                        {
                            handlerBuilder.AdditionalHandlers.Add(_twoFailOneSuccessHandler);
                        });
                    });
            });
        });

        var client = factory.Services
            .GetRequiredService<IHttpClientFactory>()
            .CreateClient("Frankfurter");

        // Act
        var response = await client.GetAsync("/latest");

        // Assert
        await Assert.That(response.IsSuccessStatusCode).IsTrue();
        await Assert.That(_twoFailOneSuccessHandler.CallCount).IsEqualTo(3);
    }

    [Test]
    public async Task CanBreak()
    {
        // Arrange
        var client = Factory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton<DelegatingHandler>(_alwaysFailHandler);
                    services.PostConfigure<HttpClientFactoryOptions>("Frankfurter", opts =>
                        opts.HttpMessageHandlerBuilderActions.Add(bld =>
                            bld.AdditionalHandlers.Add(_alwaysFailHandler)
                        )
                    );
                });
            })
            .Services
            .GetRequiredService<IHttpClientFactory>()
            .CreateClient("Frankfurter");

        // Act
        var first = await client.GetAsync("/latest");

        // Assert
        await Assert.That(first.IsSuccessStatusCode).IsFalse();
        await Assert.That(_alwaysFailHandler.CallCount).IsEqualTo(4);

        await Assert.ThrowsAsync<BrokenCircuitException>(() =>
            client.GetAsync("/latest")
        );

        await Assert.That(_alwaysFailHandler.CallCount).IsEqualTo(5);
    }
}
