using System.Xml.Linq;

using Ical.Net.CalendarComponents;
using Ical.Net.Serialization;

namespace CalDavNet;

public class Calendar : IEntity
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
            if (_displayName is null && Properties.TryGetValue(XNames.DisplayName, out var element))
            {
                _displayName = element.Value;
            }

            return _displayName;
        }
    }

    public string? Description
    {
        get
        {
            if (_description is null && Properties.TryGetValue(XNames.CalendarDescription, out var element))
            {
                _description = element.Value;
            }

            return _description;
        }
    }

    public string? Color
    {
        get
        {
            if (_color is null && Properties.TryGetValue(XNames.CalendarColor, out var element))
            {
                _color = element.Value;
            }

            return _color;
        }
    }

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

    public CalendarComponent SupportedCalendarComponentSet
    {
        get
        {
            if (Properties.TryGetValue(XNames.SupportedCalendarComponentSet, out var element))
            {
                foreach (var comp in element.Elements(XNames.Comp))
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

    public IReadOnlyDictionary<XName, XElement> Properties { get; }

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
}
