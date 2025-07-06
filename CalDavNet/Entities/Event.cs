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

    public string Href { get; } = null!;

    public IReadOnlyDictionary<XName, XElement> Properties { get; }

    public Ical.Net.Calendar? ICalCalendar { get; }

    public Ical.Net.CalendarComponents.CalendarEvent? ICalEvent { get; }

    public Event(MultistatusEntry entry)
    {
        Href = entry.Href;
        Properties = entry.Properties;

        if (!string.IsNullOrEmpty(CalendarData))
        {
            ICalCalendar = Ical.Net.Calendar.Load(CalendarData);
            ICalEvent = ICalCalendar.Events.SingleOrDefault();
        }
    }

    public Task<bool> UpdateAsync(Client client, CancellationToken cancellationToken = default)
    {
        if (Etag == null || CalendarData == null)
            throw new InvalidOperationException("Event data was not loaded.");

        return client.UpdateEventAsync(Href, Etag, Serialize(), cancellationToken);
    }

    public Task<bool> DeleteAsync(Client client, CancellationToken cancellationToken = default)
    {
        if (Etag == null)
            throw new InvalidOperationException("Etag was not loaded.");

        return client.DeleteEventAsync(Href, Etag, cancellationToken);
    }

    public string Serialize()
    {
        return Calendar.CalendarSerializer.SerializeToString(ICalCalendar);
    }
}
