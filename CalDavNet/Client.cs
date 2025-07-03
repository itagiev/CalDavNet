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

    public async Task<List<Calendar>> GetCalendarsAsync(
        string calendarHomeSet,
        XDocument body,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(CalDavClient.Propfind, calendarHomeSet)
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

    public async Task<Calendar?> GetCalendarByUriAsync(string uri,
        XDocument body,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(CalDavClient.Propfind, uri)
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

    public async Task<List<Event>> GetEventsAsync(string uri, XDocument body,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(CalDavClient.Report, uri)
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

    public async Task<Event?> GetEventAsync(string uri, XDocument body,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(CalDavClient.Report, uri)
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

    public Task<Event?> GetEventAsync(string uri, string eventUri,
        CancellationToken cancellationToken = default)
        => GetEventAsync(uri, BuildBodyHelper.BuildCalendarMultigetBody(eventUri));

    public async Task<bool> CreateEventAsync(string uri, string uid, string body,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, $"{uri}{uid}.ics")
            .WithBasicAuthorization(_token);

        request.Content = new StringContent(body, Encoding.UTF8, "text/calendar");

        var response = await _client.PutAsync(request, cancellationToken);
        return response.IsSuccess;
    }

    public async Task<bool> UpdateEventAsync(string uri, string etag, string body,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, uri)
            .WithETag(etag)
            .WithBasicAuthorization(_token);

        request.Content = new StringContent(body, Encoding.UTF8, "text/calendar");

        var response = await _client.PutAsync(request, cancellationToken);
        return response.IsSuccess;
    }

    public async Task<bool> DeleteEventAsync(string uri, string etag,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, uri)
            .WithETag(etag)
            .WithBasicAuthorization(_token);

        var response = await _client.DeleteAsync(request, cancellationToken);
        return response.IsSuccess;
    }
}
