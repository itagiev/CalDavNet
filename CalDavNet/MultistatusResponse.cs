using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace CalDavNet;

public class MultistatusResponse : Response
{
    private static readonly Regex StatusCodeRegex = new Regex(@"\b(\d{3})\b", RegexOptions.Compiled);

    public IReadOnlyCollection<MultistatusEntry>? Entries { get; private set; }

    public MultistatusResponse(int statusCode, string content)
        : base(statusCode)
    {
        if (IsSuccess && TryParseDocument(content, out var document) && document.Root is not null)
        {
            Entries = document.Root.Elements().Select(ParseEntry).ToList().AsReadOnly();
        }
    }

    private static MultistatusEntry ParseEntry(XElement response)
    {
        string? status = response.Descendants(Constants.Status).FirstOrDefault()?.Value;
        int statusCode = -1;

        if (!string.IsNullOrEmpty(status))
        {
            Match match = StatusCodeRegex.Match(status);
            if (match.Success)
            {
                statusCode = int.Parse(match.Value);
            }
        }

        return new MultistatusEntry
        {
            Href = response.Element(Constants.Href)?.Value,
            StatusCode = statusCode,
            CalendarData = response.Descendants(Constants.CalendarData).FirstOrDefault()?.Value,
            ETag = response.Descendants(Constants.Getetag).FirstOrDefault()?.Value
        };
    }

    private static bool TryParseDocument(string text, [NotNullWhen(true)] out XDocument? document)
    {
        try
        {
            document = XDocument.Parse(text);
            return true;
        }
        catch
        {
            document = null;
            return false;
        }
    }
}
