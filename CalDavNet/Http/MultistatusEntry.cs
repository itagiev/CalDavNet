using System.Xml.Linq;

namespace CalDavNet;

public class MultistatusEntry
{
    private readonly Dictionary<XName, PropResponse> _properties;

    public string Href { get; init; } = null!;

    public int StatusCode { get; init; }

    public MultistatusEntry(string href, Dictionary<XName, PropResponse> properties, int statusCode = 200)
    {
        Href = href;
        _properties = properties;
        StatusCode = statusCode;
    }

    public IReadOnlyDictionary<XName, PropResponse> Properties => _properties;

    public bool IsSuccessful => StatusCode >= 200 && StatusCode <= 299;

    public bool IsCalendar => Properties.TryGetValue(XNames.ResourceType, out var prop)
        && prop.IsSuccessful
        && prop.Prop.Element(XNames.Calendar) is not null;
}
