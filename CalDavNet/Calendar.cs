using System.Xml.Linq;

namespace CalDavNet;

// TODO: lazy loading
// TODO: add properties
public class Calendar
{
    private readonly IReadOnlyDictionary<XName, XElement> _properties;

    public string Uri { get; } = null!;

    public string? DisplayName => _properties.TryGetValue(XNames.DisplayName, out var element)
        ? element.Value
        : null;

    public string? CTag => _properties.TryGetValue(XNames.GetCTag, out var element)
        ? element.Value
        : null;

    public string? SyncToken => _properties.TryGetValue(XNames.SyncToken, out var element)
        ? element.Value
        : null;

    public Calendar(MultistatusEntry entry)
    {
        Uri = entry.Uri;
        _properties = entry.Properties;
    }
}
