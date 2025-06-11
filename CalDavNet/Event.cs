using System.Xml.Linq;

namespace CalDavNet;

public class Event
{
    private readonly IReadOnlyDictionary<XName, XElement> _properties;

    private string? _etag;

    public string Uri { get; } = null!;

    public string? ETag
    {
        get
        {
            if (_etag is null && _properties.ContainsKey(XNames.GetETag))
            {
                _etag = _properties[XNames.GetETag].Value;
            }

            return _etag;
        }
    }

    public Event(MultistatusEntry entry)
    {
        Uri = entry.Uri;
        _properties = entry.Properties;
    }
}
