namespace CalDavNet;

public class CalDavClient
{
    public static readonly HttpMethod Propfind = new HttpMethod("PROPFIND");
    public static readonly HttpMethod Proppatch = new HttpMethod("PROPPATCH");
    public static readonly HttpMethod Report = new HttpMethod("REPORT");
    public static readonly HttpMethod Mkcalendar = new HttpMethod("MKCALENDAR");

    private readonly IHttpClientFactory _clientFactory;
    private readonly string _clientName;

    public CalDavClient(IHttpClientFactory clientFactory, string clientName)
    {
        _clientFactory = clientFactory;
        _clientName = clientName;
    }

    public async Task<MultistatusResponse> SendForMultiResponseAsync(HttpRequestMessage request,
        CancellationToken cancellationToken = default)
    {
        var httpClient = _clientFactory.CreateClient(_clientName);
        var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return new MultistatusResponse((int)response.StatusCode, content);
    }

    public async Task<Response> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        var httpClient = _clientFactory.CreateClient(_clientName);
        var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        return new Response((int)response.StatusCode);
    }
}
