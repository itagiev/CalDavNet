using System.Xml.Linq;

namespace CalDavNet;

public interface IEntity
{
    IReadOnlyDictionary<XName, PropResponse> Properties { get; }
}
