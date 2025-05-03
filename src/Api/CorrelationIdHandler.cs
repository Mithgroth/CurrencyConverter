namespace Api;

public class CorrelationIdHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        const string header = "X-Correlation-ID";

        var context = httpContextAccessor.HttpContext;
        if (context != null && context.Items.TryGetValue(header, out var correlationIdObj) && correlationIdObj is string correlationId)
        {
            if (!request.Headers.Contains(header))
                request.Headers.Add(header, correlationId);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
