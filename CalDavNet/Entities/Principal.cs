using System.Xml.Linq;

namespace CalDavNet;

public class Principal : IEntity
{
    private string? _calendarHomeSet;

    public string Uri { get; } = null!;

    public string? CalendarHomeSet
    {
        get
        {
            if (_calendarHomeSet is null && Properties.TryGetValue(XNames.CalendarHomeSet, out var element))
            {
                _calendarHomeSet = element.Value;
            }

            return _calendarHomeSet;
        }
    }

    public Principal(MultistatusEntry entry)
    {
        Uri = entry.Uri;
        Properties = entry.Properties;
    }

    public IReadOnlyDictionary<XName, XElement> Properties { get; }
}
