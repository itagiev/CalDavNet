namespace CalDavNet;

public class CalDavClient
{
    public static readonly HttpMethod Propfind = new HttpMethod("PROPFIND");
    public static readonly HttpMethod Report = new HttpMethod("REPORT");

    private readonly IHttpClientFactory _clientFactory;
    private readonly string _clientName;

    public CalDavClient(IHttpClientFactory clientFactory, string clientName)
    {
        _clientFactory = clientFactory;
        _clientName = clientName;
    }

    public async Task<MultistatusResponse> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        var httpClient = _clientFactory.CreateClient(_clientName);
        var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return new MultistatusResponse((int)response.StatusCode, content);
    }

    public async Task<Response> PutAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        var httpClient = _clientFactory.CreateClient(_clientName);
        var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        return new Response((int)response.StatusCode);
    }

    public async Task<Response> DeleteAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        var httpClient = _clientFactory.CreateClient(_clientName);
        var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        return new Response((int)response.StatusCode);
    }
}
