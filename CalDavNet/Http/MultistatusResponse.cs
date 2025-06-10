using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;

namespace CalDavNet;

public class MultistatusResponse : Response
{
    public IReadOnlyList<MultistatusEntry> Entries { get; private set; } = null!;

    public MultistatusResponse(int statusCode, string content)
        : base(statusCode)
    {
        if (IsSuccess && TryParseDocument(content, out var element) && element is not null)
        {
            Entries = element.Elements()
                .Select(ParseEntry)
                .ToList()
                .AsReadOnly();
        }
        else
        {
            Entries = [];
        }
    }

    private static MultistatusEntry ParseEntry(XElement response)
    {
        return new MultistatusEntry
        {
            Uri = response.Element(XNames.Href)!.Value,
            Properties = response.Elements()
                .Where(x => x.Name == XNames.Propstat)
                .Where(x =>
                {
                    int statusCode = x.GetStatusCode();
                    return statusCode >= 200 && statusCode <= 299;
                })
                .SelectMany(x => x.Elements()
                    .Where(x => x.Name == XNames.Prop)
                    .Elements())
                .ToDictionary(x => x.Name, x => x)
                .AsReadOnly()
        };
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
}
