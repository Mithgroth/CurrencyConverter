using System.Text.Json;

namespace Api.Providers.Frankfurter;

public static class FrankfurterJsonOptions
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };
}
