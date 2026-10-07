using NotoriousClient.Sender;

namespace NotoriousSkeleton.Catalog.Integration.Tests.Framework;

public class TestRequestSender : IRequestSender
{
    private readonly HttpClient _client;

    public TestRequestSender(HttpClient client)
    {
        _client = client;
    }
    public HttpResponseMessage Send(HttpRequestMessage request)
    {
        return _client.Send(request);
    }

    public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        return _client.SendAsync(request, cancellationToken);
    }
}
