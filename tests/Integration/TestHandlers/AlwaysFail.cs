using System.Net;

namespace Integration.TestHandlers;

public class AlwaysFail : DelegatingHandler
{
    public int CallCount { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        CallCount++;
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError));
    }
}
