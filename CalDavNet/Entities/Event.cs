using System.Xml.Linq;

namespace CalDavNet;

public class Event : IEntity
{
    private string? _etag;
    private string? _calendarData;

    public string? Etag
    {
        get
        {
            if (_etag is null && Properties.TryGetValue(XNames.GetEtag, out var element))
            {
                _etag = element.Value;
            }

            return _etag;
        }
    }

    public string? CalendarData
    {
        get
        {
            if (_calendarData is null && Properties.TryGetValue(XNames.CalendarData, out var element))
            {
                _calendarData = element.Value;
            }

            return _calendarData;
        }
    }

    public Event(MultistatusEntry entry)
    {
        Uri = entry.Uri;
        Properties = entry.Properties;
    }

    public string Uri { get; } = null!;

    public IReadOnlyDictionary<XName, XElement> Properties { get; }

    // TODO: implement
    public Task Update(Client client, CancellationToken cancellationToken = default)
    {
        if (Etag == null || CalendarData == null)
            throw new InvalidOperationException("Event data was not loaded.");

        return client.UpdateEventAsync(Uri, Etag, "", cancellationToken);
    }

    public Task Delete(Client client, CancellationToken cancellationToken = default)
    {
        if (Etag == null)
            throw new InvalidOperationException("ETag was not loaded.");

        return client.DeleteEventAsync(Uri, Etag, cancellationToken);
    }
}
