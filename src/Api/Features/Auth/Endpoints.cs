namespace Api.Features.Auth;

public static class Endpoints
{
    public static WebApplication MapAuthEndpoints(this WebApplication app)
    {
        var v1 = app.MapGroup("/api/v1");

        v1.MapPost("/auth/login", (LoginRequest login, IConfiguration config) =>
        {
            var users = config.GetSection("Users").Get<List<AppUser>>()!;
            var user = users.FirstOrDefault(u =>
                string.Equals(u.Name, login.Name, StringComparison.OrdinalIgnoreCase)
                    && u.Password == login.Password);
            if (user is null)
            {
                return Results.Unauthorized();
            }

            var token = Service.GenerateToken(user.Name, user.Role, config);
            return Results.Ok(new { token });
        });

        return app;
    }
}
