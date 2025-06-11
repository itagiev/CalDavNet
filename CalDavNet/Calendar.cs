using System.Xml.Linq;

namespace CalDavNet;

public class Calendar
{
    private readonly IReadOnlyDictionary<XName, XElement> _properties;

    private string? _displayName;
    private string? _ctag;
    private string? _syncToken;

    public string Uri { get; } = null!;

    public string? DisplayName
    {
        get
        {
            if (_displayName is null && _properties.ContainsKey(XNames.DisplayName))
            {
                _displayName = _properties[XNames.DisplayName].Value;
            }

            return _displayName;
        }
    }

    public string? CTag
    {
        get
        {
            if (_ctag is null && _properties.ContainsKey(XNames.GetCTag))
            {
                _ctag = _properties[XNames.GetCTag].Value;
            }

            return _ctag;
        }
    }

    public string? SyncToken
    {
        get
        {
            if (_syncToken is null && _properties.ContainsKey(XNames.SyncToken))
            {
                _syncToken = _properties[XNames.SyncToken].Value;
            }

            return _syncToken;
        }
    }

    public Calendar(MultistatusEntry entry)
    {
        Uri = entry.Uri;
        _properties = entry.Properties;
    }
}
