using System.Xml.Linq;

namespace CalDavNet.Extensions;

public static class XElementExtensions
{
    public static XElement? LocalNameElement(this XElement element, string localName, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
    {
        return element.Elements().FirstOrDefault(x => x.Name.LocalName.Equals(localName, comparison));
    }

    public static IEnumerable<XElement> LocalNameElements(this XElement element, string localName, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
    {
        return element.Elements().Where(x => x.Name.ToString().Equals(localName, comparison));
    }
}
