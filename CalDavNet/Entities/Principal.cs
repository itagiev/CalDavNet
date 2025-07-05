using System.Xml.Linq;

namespace CalDavNet;

public class Principal : IEntity
{
    private string? _displayName;
    private string? _calendarHomeSet;

    public string Href { get; } = null!;

    public string? DisplayName
    {
        get
        {
            if (_displayName is null && Properties.TryGetValue(XNames.DisplayName, out var element))
            {
                _displayName = element.Value;
            }

            return _displayName;
        }
    }

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
        Href = entry.Href;
        Properties = entry.Properties;
    }

    public IReadOnlyDictionary<XName, XElement> Properties { get; }
}
