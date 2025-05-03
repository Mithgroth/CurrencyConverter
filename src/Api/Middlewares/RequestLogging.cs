using System.Diagnostics;

namespace Api.Middlewares;

public class RequestLogging(RequestDelegate next, ILogger<RequestLogging> logger)
{
    public async Task Invoke(HttpContext context)
    {
        var sw = Stopwatch.StartNew();

        await next(context);

        sw.Stop();

        var ip = context.Connection.RemoteIpAddress?.ToString();
        var method = context.Request.Method;
        var path = context.Request.Path;
        var statusCode = context.Response.StatusCode;
        var clientId = context.User.FindFirst("client_id")?.Value ?? context.User.FindFirst("sub")?.Value;

        logger.LogInformation("Request {Method} {Path} from IP {IP} | ClientId {ClientId} | Status {StatusCode} | Took {Elapsed}ms",
            method, path, ip, clientId, statusCode, sw.ElapsedMilliseconds);
    }
}
