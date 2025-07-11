using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace CalDavNet;

public static class XElementExtensions
{
    private static readonly Regex StatusCodeRegex = new Regex(@"\b(\d{3})\b", RegexOptions.Compiled);

    public static int GetStatusCodeOrDefault(this XElement element, int @default = -1)
    {
        string? status = element.Element(XNames.Status)?.Value;

        if (string.IsNullOrEmpty(status))
        {
            return @default;
        }

        Match match = StatusCodeRegex.Match(status);

        if (match.Success && int.TryParse(match.Value, out var code))
        {
            return code;
        }

        return @default;
    }
}
