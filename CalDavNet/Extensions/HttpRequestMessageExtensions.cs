using System.Net.Http.Headers;

namespace CalDavNet;

public static class HttpRequestMessageExtensions
{
    public static HttpRequestMessage WithBasicAuthorization(this HttpRequestMessage request, string token)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", token);
        return request;
    }

    public static HttpRequestMessage WithPrefer(this HttpRequestMessage request, string prefer)
    {
        request.Headers.Add("Prefer", prefer);
        return request;
    }

    public static HttpRequestMessage WithDepth(this HttpRequestMessage request, int depth)
    {
        request.Headers.Add("Depth", depth.ToString());
        return request;
    }

    public static HttpRequestMessage WithEtag(this HttpRequestMessage request, string etag)
    {
        request.Headers.Add("If-Match", $"\"{etag}\"");
        return request;
    }
}
