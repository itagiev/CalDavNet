using System.Xml.Linq;

using Ical.Net.CalendarComponents;
using Ical.Net.Serialization;

namespace CalDavNet;

public class Calendar : IEntity
{
    internal static readonly CalendarSerializer CalendarSerializer = new CalendarSerializer();

    private string? _ctag;
    private string? _syncToken;
    private string? _displayName;
    private string? _timezone;

    public string? Ctag
    {
        get
        {
            if (_ctag is null && Properties.TryGetValue(XNames.GetCtag, out var element))
            {
                _ctag = element.Value;
            }

            return _ctag;
        }
    }

    public string? SyncToken
    {
        get
        {
            if (_syncToken is null && Properties.TryGetValue(XNames.SyncToken, out var element))
            {
                _syncToken = element.Value;
            }

            return _syncToken;
        }
    }

    public string? DisplayName
    {
        get
        {
            if (_displayName is null && Properties.TryGetValue(XNames.DisplayName, out var element))
            {
                _displayName = element.Value;
            }

            return _displayName;
        }
    }

    public string? Timezone
    {
        get
        {
            if (_timezone is null && Properties.TryGetValue(XNames.CalendarTimezone, out var element))
            {
                _timezone = element.Value;
            }

            return _timezone;
        }
    }

    public string Href { get; } = null!;

    public IReadOnlyDictionary<XName, XElement> Properties { get; }

    private readonly Ical.Net.Calendar _calendar;

    public Calendar(MultistatusEntry entry)
    {
        Href = entry.Href;
        Properties = entry.Properties;
        _calendar = new Ical.Net.Calendar();
        _calendar.AddTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time"));
    }

    public async Task<Event?> CreateEventAsync(Client client, CalendarEvent @event,
        CancellationToken cancellationToken = default)
    {
        var eventHref = $"{Href}{@event.Uid}.ics";
        var result = await client.CreateEventAsync(eventHref, CalendarSerializer.SerializeToString(@event), cancellationToken);

        if (result)
        {
            return await GetEventAsync(client, eventHref, cancellationToken);
        }

        return null;
    }

    public Task<Event?> GetEventAsync(Client client, XDocument body, CancellationToken cancellationToken = default)
    {
        return client.GetEventAsync(Href, body, cancellationToken);
    }

    public Task<Event?> GetEventAsync(Client client, string eventHref, CancellationToken cancellationToken = default)
    {
        return client.GetEventAsync(Href, eventHref, cancellationToken);
    }

    //public async Task<bool> CreateEventAsync(Client client, CalendarEvent @event)
    //{

    //    var serializer = new CalendarSerializer();
    //    var body = serializer.SerializeToString(calendar);

    //    await client.CreateEventAsync(Href, uid, body);
    //}

    //public async Task<Event> GetEventsAsync(Client client)
    //{
    //    throw new NotImplementedException();
    //}
}
