using System.Xml.Linq;

namespace CalDavNet;

// TODO: lazy loading
// TODO: add properties
public class Principal
{
    private readonly IReadOnlyDictionary<XName, XElement> _properties;

    public string Uri { get; } = null!;

    public string? CalendarHomeSet => _properties.TryGetValue(XNames.CalendarHomeSet, out var element)
        ? element.Value
        : null;

    public Principal(MultistatusEntry entry)
    {
        Uri = entry.Uri;
        _properties = entry.Properties;
    }
}
