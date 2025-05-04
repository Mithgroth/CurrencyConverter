using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using TUnit.Core.Interfaces;

namespace Integration.Utilities;

public class WebApplicationFactory : WebApplicationFactory<Program>, IAsyncInitializer
{
    public Task InitializeAsync()
    {
        _ = Server;

        return Task.CompletedTask;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IConfigureOptions<JwtBearerOptions>>();
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestHandlers.Auth>("Test", _ => { });

            // override the rate–limiter so it never rejects
            services.PostConfigure<RateLimiterOptions>(opts =>
            {
                opts.GlobalLimiter = PartitionedRateLimiter
                    .Create<HttpContext, string>(ctx =>
                        RateLimitPartition.GetNoLimiter(
                            ctx.User.Identity?.Name
                            ?? ctx.Request.Headers.Host.ToString()
                        )
                    );
                // optional: bump RejectionStatusCode so tests can’t hit 429
                opts.RejectionStatusCode = StatusCodes.Status200OK;
            });
        });
    }
}
