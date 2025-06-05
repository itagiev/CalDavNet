namespace CalDavNet;

public class MultistatusEntry
{
    public string? Href { get; init; }

    public int StatusCode { get; init; }

    public string? CalendarData { get; init; }

    public string? ETag { get; init; }
}
