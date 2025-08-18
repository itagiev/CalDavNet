using System.Xml.Linq;

using Ical.Net.CalendarComponents;
using Ical.Net.Serialization;

namespace CalDavNet;

public class Calendar
{
    internal static readonly CalendarSerializer CalendarSerializer = new CalendarSerializer();

    private readonly Ical.Net.Calendar _calendar;

    private string? _displayName;
    private string? _description;
    private string? _color;
    private string? _ctag;
    private string? _syncToken;
    private CalendarComponent _supportedCalendarComponentSet = CalendarComponent.None;

    public string? DisplayName
    {
        get
        {
            if (_displayName is null
                && Properties.TryGetValue(XNames.DisplayName, out var prop)
                && prop.IsSuccessStatusCode)
            {
                _displayName = prop.Prop.Value;
            }

            return _displayName;
        }
    }

    public string? Description
    {
        get
        {
            if (_description is null
                && Properties.TryGetValue(XNames.CalendarDescription, out var prop)
                && prop.IsSuccessStatusCode)
            {
                _description = prop.Prop.Value;
            }

            return _description;
        }
    }

    public string? Color
    {
        get
        {
            if (_color is null
                && Properties.TryGetValue(XNames.CalendarColor, out var prop)
                && prop.IsSuccessStatusCode)
            {
                _color = prop.Prop.Value;
            }

            return _color;
        }
    }

    public string? Ctag
    {
        get
        {
            if (_ctag is null
                && Properties.TryGetValue(XNames.GetCtag, out var prop)
                && prop.IsSuccessStatusCode)
            {
                _ctag = prop.Prop.Value;
            }

            return _ctag;
        }
    }

    public string? SyncToken
    {
        get
        {
            if (_syncToken is
                null && Properties.TryGetValue(XNames.SyncToken, out var prop)
                && prop.IsSuccessStatusCode)
            {
                _syncToken = prop.Prop.Value;
            }

            return _syncToken;
        }
    }

    public CalendarComponent SupportedCalendarComponentSet
    {
        get
        {
            if (_supportedCalendarComponentSet == CalendarComponent.None
                && Properties.TryGetValue(XNames.SupportedCalendarComponentSet, out var prop)
                && prop.IsSuccessStatusCode)
            {
                foreach (var comp in prop.Prop.Elements(XNames.Comp))
                {
                    if (comp.Attribute("name") is XAttribute attr)
                    {
                        switch (attr.Value)
                        {
                            case Constants.Comp.VEVENT:
                                _supportedCalendarComponentSet |= CalendarComponent.VEVENT;
                                break;
                            case Constants.Comp.VTODO:
                                _supportedCalendarComponentSet |= CalendarComponent.VTODO;
                                break;
                            case Constants.Comp.VJOURNAL:
                                _supportedCalendarComponentSet |= CalendarComponent.VJOURNAL;
                                break;
                            case Constants.Comp.VFREEBUSY:
                                _supportedCalendarComponentSet |= CalendarComponent.VFREEBUSY;
                                break;
                            case Constants.Comp.VALARM:
                                _supportedCalendarComponentSet |= CalendarComponent.VALARM;
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            return _supportedCalendarComponentSet;
        }
    }

    public string Href { get; } = null!;

    public IReadOnlyDictionary<XName, PropResponse> Properties { get; }

    public Calendar(MultistatusEntry entry)
    {
        Href = entry.Href;
        Properties = entry.Properties;
        _calendar = new Ical.Net.Calendar();
        _calendar.AddTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time"));
    }

    public Task<bool> CreateEventAsync(Client client, CalendarEvent @event,
        CancellationToken cancellationToken = default)
    {
        _calendar.Events.Clear();
        _calendar.Events.Add(@event);

        var eventHref = $"{Href}{@event.Uid}.ics";
        return client.CreateEventAsync(eventHref, CalendarSerializer.SerializeToString(_calendar), cancellationToken);
    }

    public bool IsComponentSupported(CalendarComponent component)
    {
        return (component & SupportedCalendarComponentSet) == component;
    }

    public Task<List<Event>> GetEventsAsync(Client client, XDocument body, CancellationToken cancellationToken = default)
    {
        return client.GetEventsAsync(Href, body, cancellationToken);
    }

    public Task<Event?> GetEventAsync(Client client, XDocument body, CancellationToken cancellationToken = default)
    {
        return client.GetEventAsync(Href, body, cancellationToken);
    }

    public Task<Event?> GetEventAsync(Client client, string eventHref, CancellationToken cancellationToken = default)
    {
        return client.GetEventAsync(Href, eventHref, cancellationToken);
    }

    public Task<SyncItemCollection> SyncItemsAsync(Client client, string? syncToken,
        CancellationToken cancellationToken = default)
    {
        return client.SyncCalendarItemsAsync(Href, syncToken, cancellationToken);
    }

    public Task<bool> DeleteAsync(Client client, CancellationToken cancellationToken = default)
    {
        if (Ctag is null)
            throw new InvalidOperationException("Ctag was not loaded.");

        return client.DeleteAsync(Href, Ctag, cancellationToken);
    }
}
