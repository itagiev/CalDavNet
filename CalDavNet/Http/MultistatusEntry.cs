using System.Xml.Linq;

namespace CalDavNet;

public class MultistatusEntry
{
    public string Href { get; init; } = null!;

    public IReadOnlyDictionary<XName, XElement> Properties { get; init; } = null!;

    public bool IsCalendar =>
        Properties.TryGetValue(XNames.ResourceType, out var rt)
        && rt.Element(XNames.Calendar) != null;
}
