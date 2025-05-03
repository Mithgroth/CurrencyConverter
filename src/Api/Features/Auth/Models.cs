namespace Api.Features.Auth;

public record LoginRequest(string Name, string Password);

public class AppUser
{
    public string Name { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
