using System.Xml.Linq;

namespace CalDavNet;

public interface IEntity
{
    IReadOnlyDictionary<XName, XElement> Properties { get; }
}
