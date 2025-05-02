using Microsoft.AspNetCore.Mvc.Testing;
using TUnit.Core.Interfaces;

namespace Integration;

public class WebApplicationFactory : WebApplicationFactory<Program>, IAsyncInitializer
{
    public Task InitializeAsync()
    {
        _ = Server;

        return Task.CompletedTask;
    }
}