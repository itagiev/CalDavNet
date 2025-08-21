using System.Xml.Linq;

namespace CalDavNet;

public class Event
{
    private string? _etag;
    private string? _calendarData;

    public string? Etag
    {
        get
        {
            if (_etag is null &&
                Properties.TryGetValue(XNames.GetEtag, out var prop)
                && prop.IsSuccessStatusCode)
            {
                _etag = prop.Prop.Value;
            }

            return _etag;
        }
    }

    public string? CalendarData
    {
        get
        {
            if (_calendarData is null &&
                Properties.TryGetValue(XNames.CalendarData, out var prop)
                && prop.IsSuccessStatusCode)
            {
                _calendarData = prop.Prop.Value;
            }

            return _calendarData;
        }
    }

    public string Href { get; } = null!;

    public IReadOnlyDictionary<XName, PropResponse> Properties { get; }

    public Ical.Net.Calendar? ICalCalendar { get; }

    public Ical.Net.CalendarComponents.CalendarEvent? ICalEvent { get; }

    public Event(MultistatusEntry entry)
    {
        Href = entry.Href;
        Properties = entry.Properties;

        if (!string.IsNullOrEmpty(CalendarData))
        {
            ICalCalendar = Ical.Net.Calendar.Load(CalendarData);
            if (ICalCalendar is not null)
            {
                ICalEvent = ICalCalendar.Events.SingleOrDefault();
            }
        }
    }

    public Task<bool> UpdateAsync(Client client, CancellationToken cancellationToken = default)
    {
        if (Etag is null || CalendarData is null)
            throw new InvalidOperationException("Etag or calendar data was not loaded.");

        return client.UpdateEventAsync(Href, Etag, Serialize()!, cancellationToken);
    }

    public Task<bool> DeleteAsync(Client client, CancellationToken cancellationToken = default)
    {
        if (Etag is null)
            throw new InvalidOperationException("Etag was not loaded.");

        return client.DeleteAsync(Href, Etag, cancellationToken);
    }

    public string? Serialize()
    {
        return Calendar.CalendarSerializer.SerializeToString(ICalCalendar);
    }
}
