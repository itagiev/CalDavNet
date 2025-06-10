using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace CalDavNet;

public static class XElementExtensions
{
    private static readonly Regex StatusCodeRegex = new Regex(@"\b(\d{3})\b", RegexOptions.Compiled);

    public static int GetStatusCode(this XElement element)
    {
        string? status = element.Element(XName.Get(Constants.Dav.Status, Constants.Dav.Namespace))?.Value;

        if (string.IsNullOrEmpty(status))
        {
            return -1;
        }

        Match match = StatusCodeRegex.Match(status);

        if (match.Success && int.TryParse(match.Value, out var code))
        {
            return code;
        }

        return -1;
    }
}
