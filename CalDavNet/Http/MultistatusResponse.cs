using System.Xml.Linq;

namespace CalDavNet;

public class MultistatusResponse : Response
{
    readonly List<MultistatusEntry> _entries = null!;

    public IReadOnlyList<MultistatusEntry> Entries => _entries;

    public string? SyncToken { get; }

    public MultistatusResponse(int statusCode, string content)
        : base(statusCode)
    {
        var root = XElement.Parse(content);

        if (IsSuccessStatusCode)
        {
            _entries = root.Elements(XNames.Response).Select(ParseResponse).ToList();
            if (root.Element(XNames.SyncToken) is XElement element)
            {
                SyncToken = element.Value;
            }
        }
        else
        {
            _entries = [];
        }
    }

    private static MultistatusEntry ParseResponse(XElement response)
    {
        Dictionary<XName, PropResponse> properties = [];

        foreach (var propstat in response.Elements(XNames.Propstat))
        {
            int statusCode = propstat.GetStatusCodeOrDefault();

            foreach (var prop in propstat.Elements(XNames.Prop))
            {
                foreach (var p in prop.Elements())
                {
                    properties.Add(p.Name, new PropResponse(p, statusCode));
                }
            }
        }

        return new MultistatusEntry(response.Element(XNames.Href)!.Value,
            properties,
            response.GetStatusCodeOrDefault(200));
    }
}
