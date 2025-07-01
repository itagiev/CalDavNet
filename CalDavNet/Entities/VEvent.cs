using System.Xml.Linq;

namespace CalDavNet;

public class VEvent : IEntity
{
    private string? _etag;

    private string? _calendarData;

    public string Uri { get; } = null!;

    public string? ETag
    {
        get
        {
            if (_etag is null && Properties.TryGetValue(XNames.GetETag, out var element))
            {
                _etag = element.Value;
            }

            return _etag;
        }
    }

    public string? CalendarData
    {
        get
        {
            if (_calendarData is null && Properties.TryGetValue(XNames.CalendarData, out var element))
            {
                _calendarData = element.Value;
            }

            return _calendarData;
        }
    }

    public VEvent(MultistatusEntry entry)
    {
        Uri = entry.Uri;
        Properties = entry.Properties;
    }

    public IReadOnlyDictionary<XName, XElement> Properties { get; }
}
