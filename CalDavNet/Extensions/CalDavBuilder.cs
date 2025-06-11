using Microsoft.Extensions.DependencyInjection;

namespace CalDavNet;

public class CalDavBuilder
{
    public IServiceCollection Services { get; }

    /// <summary>
    /// CalDAV server (e.g. caldav.yandex.ru)
    /// </summary>
    public Uri BaseAddress { get; set; } = null!;

    /// <summary>
    /// Default is "1"
    /// </summary>
    public string? Depth { get; set; } = "1";

    /// <summary>
    /// https://datatracker.ietf.org/doc/html/rfc7240
    /// Default is "return-minimal"
    /// </summary>
    public string? Prefer { get; set; } = "return-minimal";

    public CalDavBuilder(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        Services = services;
    }
}
