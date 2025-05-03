using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Api.Features.Auth;
using Api.Features.Currencies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Integration;

public class Auth
{
    [ClassDataSource<WebApplicationFactory>(Shared = SharedType.PerTestSession)]
    public required WebApplicationFactory WebApplicationFactory { get; init; }

    [Test]
    public async Task CanLogin()
    {
        // Arrange
        var config = WebApplicationFactory.Services.GetRequiredService<IConfiguration>();
        var users = config.GetSection("Users").Get<List<AppUser>>()!;
        var role = "Intern";
        var testUser = users.First(u => u.Role == role);

        var client = WebApplicationFactory.CreateClient();
        var login = new LoginRequest(testUser.Name, testUser.Password);

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", login);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        await Assert.That(body.TryGetProperty("token", out var token)).IsTrue();

        var jwt = token.GetString();
        await Assert.That(jwt).IsNotNullOrEmpty();

        var handler = new JwtSecurityTokenHandler();
        var parsed = handler.ReadJwtToken(jwt!);

        var roleClaim = parsed.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
        await Assert.That(roleClaim).IsNotNull();
        await Assert.That(roleClaim!.Value).IsEqualTo(role);
    }

    [Test]
    [Arguments("FinancialExpert", HttpStatusCode.OK)]
    [Arguments("Intern", HttpStatusCode.Forbidden)]
    public async Task CanRbac(string role, HttpStatusCode expectedStatusCode)
    {
        // Arrange
        var client = WebApplicationFactory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", role);

        var request = new ConvertCurrencyRequest
        {
            From = "EUR",
            To = "USD",
            Amount = 100m
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/currencies/convert", request);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(expectedStatusCode);
    }
}
