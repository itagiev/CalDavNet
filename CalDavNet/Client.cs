using System.Text;
using System.Xml.Linq;

namespace CalDavNet;

public class Client
{
    private readonly CalDavClient _caldav;
    private readonly string _token;

    public Client(CalDavClient client, string username, string password)
    {
        _caldav = client;
        _token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
    }

    public async Task<string?> GetPrincipalNameAsync(CancellationToken cancellationToken = default)
    {
        var body = BodyBuilder.BuildCurrentUserPrincipalPropfindBody();

        var request = _caldav.BuildPropfindRequestMessage("", body)
            .WithDepth(0)
            .WithBasicAuthorization(_token);

        var response = await _caldav.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (response.IsSuccess
            && response.Entries.FirstOrDefault() is MultistatusEntry entry
            && entry.Properties.TryGetValue(XNames.CurrentUserPrincipal, out var element))
        {
            return element.Value;
        }

        return null;
    }

    public async Task<Principal?> GetPrincipalAsync(string upn, CancellationToken cancellationToken = default)
    {
        var body = BodyBuilder.BuildAllPropPropfindBody();

        var request = _caldav.BuildPropfindRequestMessage(upn, body)
            .WithDepth(0)
            .WithBasicAuthorization(_token);

        var response = await _caldav.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (response.IsSuccess && response.Entries.FirstOrDefault() is MultistatusEntry entry)
            return new Principal(entry);

        return null;
    }

    public async Task<List<Calendar>?> GetCalendarsAsync(
        string calendarHomeSet,
        XDocument body,
        CancellationToken cancellationToken = default)
    {
        var request = _caldav.BuildPropfindRequestMessage(calendarHomeSet, body)
            .WithBasicAuthorization(_token);

        var response = await _caldav.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccess) return null;

        List<Calendar> calendars = [];

        foreach (var entry in response.Entries)
        {
            // Only if entry has d:resource-type with d:calendar child
            if (entry.IsCalendar)
            {
                calendars.Add(new Calendar(entry));
            }
        }

        return calendars;
    }

    public async Task<Calendar?> GetCalendarByUriAsync(string uri,
        XDocument body,
        CancellationToken cancellationToken = default)
    {
        var request = _caldav.BuildPropfindRequestMessage(uri, body)
            .WithDepth(0)
            .WithBasicAuthorization(_token);

        var response = await _caldav.SendAsync(request, cancellationToken).ConfigureAwait(false);

        // Only if success, result not empty and has d:resource-type with d:calendar child
        if (response.IsSuccess &&
            response.Entries.FirstOrDefault() is MultistatusEntry entry &&
            entry.IsCalendar)
        {
            return new Calendar(entry);
        }

        return null;
    }

    public async Task<List<Event>?> GetEventsAsync(string uri,
        XDocument body,
        CancellationToken cancellationToken = default)
    {
        var request = _caldav.BuildReportRequestMessage(uri, body)
            .WithBasicAuthorization(_token);

        var response = await _caldav.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccess)
        {
            return null;
        }

        List<Event> events = [];

        foreach (var entry in response.Entries)
        {
            events.Add(new Event(entry));
        }

        return events;
    }

    //public async Task<Event?> GetEventByUriAsync(string uri,
    //    XDocument body,
    //    CancellationToken cancellationToken = default)
    //{
    //    var request = _caldav.BuildReportRequestMessage(uri, body)
    //        .WithBasicAuthorization(_token);


    //}
}
