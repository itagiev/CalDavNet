using System.Xml.Linq;

namespace CalDavNet;

public readonly struct PropResponse(XElement prop, int statusCode)
{
    public readonly XElement Prop = prop;
    public readonly int StatusCode = statusCode;

    public readonly bool IsSuccessful => StatusCode >= 200 && StatusCode <= 299;
}
