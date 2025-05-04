using Integration.Utilities;

namespace Integration;

public abstract class TestBase
{
    [ClassDataSource<WebApplicationFactory>(Shared = SharedType.PerTestSession)]
    public required WebApplicationFactory WebApplicationFactory { get; init; }
}
