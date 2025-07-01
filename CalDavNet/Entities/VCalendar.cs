using System.Xml.Linq;

namespace CalDavNet;

public class VCalendar : IEntity
{
    private string? _displayName;
    private string? _ctag;
    private string? _syncToken;

    public string Uri { get; } = null!;

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

    public string? CTag
    {
        get
        {
            if (_ctag is null && Properties.TryGetValue(XNames.GetCTag, out var element))
            {
                _ctag = element.Value;
            }

            return _ctag;
        }
    }

    public string? SyncToken
    {
        get
        {
            if (_syncToken is null && Properties.TryGetValue(XNames.SyncToken, out var element))
            {
                _syncToken = element.Value;
            }

            return _syncToken;
        }
    }

    public VCalendar(MultistatusEntry entry)
    {
        Uri = entry.Uri;
        Properties = entry.Properties;
    }

    public IReadOnlyDictionary<XName, XElement> Properties { get; }
}
