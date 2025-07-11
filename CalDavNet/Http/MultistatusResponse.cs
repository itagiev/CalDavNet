using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;

namespace CalDavNet;

public class MultistatusResponse : Response
{
    readonly List<MultistatusEntry> _entries = null!;

    public IReadOnlyList<MultistatusEntry> Entries => _entries;

    public MultistatusResponse(int statusCode, string content)
        : base(statusCode)
    {
        _entries = (IsSuccessStatusCode && TryParseDocument(content, out var element))
            ? [.. element.Elements().Select(ParseResponse)]
            : [];
    }

    private static bool TryParseDocument(string text, [NotNullWhen(true)] out XElement? element)
    {
        try
        {
            element = XElement.Parse(text);
            return true;
        }
        catch
        {
            element = null;
            return false;
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
