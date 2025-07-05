using System.Xml.Linq;

using Ical.Net.CalendarComponents;
using Ical.Net.Serialization;

namespace CalDavNet;

public class Event : IEntity
{
    private string? _etag;
    private string? _calendarData;

    private readonly CalendarEvent? _event;

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

    public string Href { get; } = null!;

    public IReadOnlyDictionary<XName, XElement> Properties { get; }

    public Event(MultistatusEntry entry)
    {
        Href = entry.Href;
        Properties = entry.Properties;

        if (!string.IsNullOrEmpty(CalendarData))
        {
            _event = Ical.Net.Calendar.Load(CalendarData).Events.SingleOrDefault();
        }
    }

    public Task Update(Client client, CancellationToken cancellationToken = default)
    {
        if (Etag == null || CalendarData == null)
            throw new InvalidOperationException("Event data was not loaded.");

        return client.UpdateEventAsync(Href, Etag, Serialize(), cancellationToken);
    }

    public Task Delete(Client client, CancellationToken cancellationToken = default)
    {
        if (Etag == null)
            throw new InvalidOperationException("Etag was not loaded.");

        return client.DeleteEventAsync(Href, Etag, cancellationToken);
    }

    public string Serialize()
    {
        return Calendar.CalendarSerializer.SerializeToString(_event);
    }
}
