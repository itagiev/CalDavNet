using System.Xml.Linq;

namespace CalDavNet;

public class Principal
{
    private readonly IReadOnlyDictionary<XName, XElement> _properties;

    private string? _calendarHomeSet;

    public string Uri { get; } = null!;

    public string? CalendarHomeSet
    {
        get
        {
            if (_calendarHomeSet is null && _properties.ContainsKey(XNames.CalendarHomeSet))
            {
                _calendarHomeSet = _properties[XNames.CalendarHomeSet].Value;
            }

            return _calendarHomeSet;
        }
    }

    public Principal(MultistatusEntry entry)
    {
        Uri = entry.Uri;
        _properties = entry.Properties;
    }
}
