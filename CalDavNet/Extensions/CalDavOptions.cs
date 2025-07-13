namespace CalDavNet;

public class CalDavOptions
{
    /// <summary>
    /// CalDAV server (e.g. caldav.yandex.ru)
    /// </summary>
    public Uri BaseAddress { get; set; } = null!;

    /// <summary>
    /// Adds <see cref="LoggingHandler"/>.
    /// </summary>
    public bool EnableLoggingHandler { get; set; } = false;
}
