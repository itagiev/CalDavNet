using System.Xml.Linq;

namespace CalDavNet;

public class SyncItem : IEntity
{
    private string? _etag;

    public string Href { get; } = null!;

    public int StatusCode { get; }

    public string? Etag
    {
        get
        {
            if (_etag is null
                && Properties.TryGetValue(XNames.GetEtag, out var prop)
                && prop.IsSuccessful)
            {
                _etag = prop.Prop.Value;
            }

            return _etag;
        }
    }

    public IReadOnlyDictionary<XName, PropResponse> Properties { get; }

    public bool IsDeleted => StatusCode == 404;

    public SyncItem(MultistatusEntry entry)
    {
        Href = entry.Href;
        StatusCode = entry.StatusCode;
        Properties = entry.Properties;
    }
}
