using System.Text;
using System.Xml.Linq;

namespace CalDavNet;

public class Client
{
    private readonly CalDavClient _client;
    private readonly string _token;

    public Client(CalDavClient client, string username, string password)
    {
        _client = client;
        _token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
    }

    /// <summary>
    /// Gets current user principal name of a given client and credentials.
    /// </summary>
    /// <returns>Principal name (e.g. /principals/users/john@mail.com/)</returns>
    public async Task<string?> GetPrincipalNameAsync(CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(CalDavClient.Propfind, "")
            .WithDepth(0)
            .WithBasicAuthorization(_token);

        request.Content = BuildBodyHelper.BuildCurrentUserPrincipalPropfindBody().ToStringContent();

        var response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (response.IsSuccess
            && response.Entries.FirstOrDefault() is MultistatusEntry entry
            && entry.Properties.TryGetValue(XNames.CurrentUserPrincipal, out var element))
        {
            return element.Value;
        }

        return null;
    }

    /// <summary>
    /// Gets principal data,
    /// like display name, calendar home set.
    /// </summary>
    /// <param name="upn">User principal name.</param>
    public async Task<Principal?> GetPrincipalAsync(string upn, CancellationToken cancellationToken = default)
    {
        var body = BuildBodyHelper.BuildAllPropPropfindBody();

        var request = new HttpRequestMessage(CalDavClient.Propfind, upn)
            .WithDepth(0)
            .WithBasicAuthorization(_token);

        var response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (response.IsSuccess && response.Entries.FirstOrDefault() is MultistatusEntry entry)
            return new Principal(entry);

        return null;
    }

    /// <summary>
    /// Gets calendars list.
    /// </summary>
    /// <param name="href">Principal calendar home set href (e.g. /calendars/john@mail.com/).</param>
    /// <param name="body">Request body, with requested properties.</param>
    public async Task<List<Calendar>> GetCalendarsAsync(
        string href,
        XDocument body,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(CalDavClient.Propfind, href)
            .WithBasicAuthorization(_token);

        request.Content = body.ToStringContent();

        var response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccess)
            return [];

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

    /// <summary>
    /// Gets a single calendar by it's href.
    /// </summary>
    /// <param name="href">Calendar href (e.g. /calendars/john@mail.com/events-27560559/).</param>
    /// <param name="body">Request body, with requested properties.</param>
    public async Task<Calendar?> GetCalendarAsync(string href,
        XDocument body,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(CalDavClient.Propfind, href)
            .WithDepth(0)
            .WithBasicAuthorization(_token);

        request.Content = body.ToStringContent();

        var response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);

        // Only if success, result not empty and has d:resource-type with d:calendar child
        if (response.IsSuccess &&
            response.Entries.FirstOrDefault() is MultistatusEntry entry &&
            entry.IsCalendar)
        {
            return new Calendar(entry);
        }

        return null;
    }

    /// <summary>
    /// TODO:
    /// </summary>
    /// <param name="href"></param>
    /// <param name="body"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<List<Event>> GetEventsAsync(string href, XDocument body,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(CalDavClient.Report, href)
            .WithDepth(0)
            .WithBasicAuthorization(_token);

        request.Content = body.ToStringContent();

        var response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccess)
            return [];

        List<Event> events = [];

        foreach (var entry in response.Entries)
        {
            events.Add(new Event(entry));
        }

        return events;
    }

    /// <summary>
    /// Finds a single event by multiget body <see cref="BuildBodyHelper.BuildCalendarMultigetBody(string[])"/>.
    /// Request body should contain only one href (of requested event), method returns only first event even if more requested.
    /// </summary>
    /// <param name="href">Calendar href.</param>
    /// <param name="body">Multiget body.</param>
    public async Task<Event?> GetEventAsync(string href, XDocument body,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(CalDavClient.Report, href)
            .WithDepth(0)
            .WithBasicAuthorization(_token);

        request.Content = body.ToStringContent();

        var response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccess)
            return null;

        return response.Entries.Count > 0
            ? new Event(response.Entries.First())
            : null;
    }

    /// <summary>
    /// Finds a single event by it's href.
    /// </summary>
    /// <param name="calendarHref">Calendar href.</param>
    /// <param name="eventHref">Event href.</param>
    public Task<Event?> GetEventAsync(string calendarHref, string eventHref,
        CancellationToken cancellationToken = default)
        => GetEventAsync(calendarHref, BuildBodyHelper.BuildCalendarMultigetBody(eventHref));

    /// <summary>
    /// Creates calendar event.
    /// </summary>
    /// <param name="href">Event href (e.g. /calendars/john@mail.com/events-27560559/oeibm394kvocows3lgjn.ics).</param>
    /// <param name="body">Event body.</param>
    public async Task<bool> CreateEventAsync(string href, string body,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, href)
            .WithBasicAuthorization(_token);

        request.Content = new StringContent(body, Encoding.UTF8, "text/calendar");

        var response = await _client.PutAsync(request, cancellationToken);
        return response.IsSuccess;
    }

    /// <summary>
    /// Updates calendar event.
    /// </summary>
    /// <param name="href">Event href (e.g. /calendars/john@mail.com/events-27560559/oeibm394kvocows3lgjn.ics).</param>
    /// <param name="etag">Event "version"</param>
    /// <param name="body">Event body.</param>
    public async Task<bool> UpdateEventAsync(string href, string etag, string body,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, href)
            .WithEtag(etag)
            .WithBasicAuthorization(_token);

        request.Content = new StringContent(body, Encoding.UTF8, "text/calendar");

        var response = await _client.PutAsync(request, cancellationToken);
        return response.IsSuccess;
    }

    /// <summary>
    /// Deletes calendar event.
    /// </summary>
    /// <param name="href">Event href (e.g. /calendars/john@mail.com/events-27560559/oeibm394kvocows3lgjn.ics).</param>
    /// <param name="etag">Event "version".</param>
    public async Task<bool> DeleteEventAsync(string href, string etag,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, href)
            .WithEtag(etag)
            .WithBasicAuthorization(_token);

        var response = await _client.DeleteAsync(request, cancellationToken);
        return response.IsSuccess;
    }
}
