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
            if (_displayName is null
                && Properties.TryGetValue(XNames.DisplayName, out var prop)
                && prop.IsSuccessful)
            {
                _displayName = prop.Prop.Value;
            }

            return _displayName;
        }
    }

    public string? CalendarHomeSet
    {
        get
        {
            if (_calendarHomeSet is null
                && Properties.TryGetValue(XNames.CalendarHomeSet, out var prop)
                && prop.IsSuccessful)
            {
                _calendarHomeSet = prop.Prop.Value;
            }

            return _calendarHomeSet;
        }
    }

    public IReadOnlyDictionary<XName, PropResponse> Properties { get; }

    public Principal(MultistatusEntry entry)
    {
        Href = entry.Href;
        Properties = entry.Properties;
    }
}
