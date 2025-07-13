using System.Xml.Linq;

namespace CalDavNet;

public record PropResponse(XElement prop, int statusCode)
{
    public readonly XElement Prop = prop;
    public readonly int StatusCode = statusCode;

    public bool IsSuccessful => StatusCode >= 200 && StatusCode <= 299;
}
