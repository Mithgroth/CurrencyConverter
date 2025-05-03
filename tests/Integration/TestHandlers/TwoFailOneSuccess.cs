using System.Net;
using System.Text;

namespace Integration.TestHandlers;

public class TwoFailOneSuccess : DelegatingHandler
{
    public int CallCount { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        CallCount++;
        if (CallCount < 3)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError));
        }

        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                "{\"rates\":{\"USD\":1.1}}",
                Encoding.UTF8,
                "application/json")
        });
    }
}
