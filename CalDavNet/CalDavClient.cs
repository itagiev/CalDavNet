using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;

namespace CalDavNet;

public class CalDavClient
{
    public static readonly HttpMethod Report = new HttpMethod("REPORT");
    public static readonly HttpMethod Propfind = new HttpMethod("PROPFIND");

    private readonly IHttpClientFactory _clientFactory;

    public CalDavClient(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public async Task<MultistatusResponse> ReportAsync(string token, string email, CancellationToken cancellationToken = default)
    {
        var httpClient = _clientFactory.CreateClient(nameof(CalDavClient));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);

        var request = new HttpRequestMessage(Report, $"calendars/{email}/events-default");
        request.Content = new StringContent(BuildRequestTemplate(), Encoding.UTF8, "application/xml");

        var response = await httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        return new MultistatusResponse((int)response.StatusCode, content);
    }

    public string BuildRequestTemplate()
    {
        var dateTimeFormat = new DateTimeFormat("yyyyMMdd'T'HHmmss'Z'");

        var now = DateTime.UtcNow;
        var startDay = new DateTime(DateOnly.FromDateTime(now), TimeOnly.MinValue);
        var endDay = new DateTime(DateOnly.FromDateTime(now), TimeOnly.MaxValue);

        return $"""
            <?xml version="1.0" encoding="utf-8" ?>
            <C:calendar-query xmlns:D="DAV:" xmlns:C="urn:ietf:params:xml:ns:caldav">
              <D:prop>
                <D:getetag/>
                  <C:calendar-data>
                  <C:comp name="VCALENDAR">
                  <C:prop name="VERSION"/>
                    <C:comp name="VEVENT">
                    <C:prop name="SUMMARY"/>
                    <C:prop name="UID"/>
                    <C:prop name="DTSTART"/>
                    <C:prop name="DTEND"/>
                    <C:prop name="DURATION"/>
                    <C:prop name="RRULE"/>
                    <C:prop name="RDATE"/>
                    <C:prop name="EXRULE"/>
                    <C:prop name="EXDATE"/>
                    <C:prop name="RECURRENCE-ID"/>
                  </C:comp>
                  <C:comp name="VTIMEZONE"/>
                  </C:comp>
                </C:calendar-data>
              </D:prop>
              <C:filter>
                <C:comp-filter name="VCALENDAR">
                  <C:comp-filter name="VEVENT">
                    <C:time-range start="{startDay.ToString(dateTimeFormat.FormatString, dateTimeFormat.FormatProvider)}"
                        end="{endDay.ToString(dateTimeFormat.FormatString, dateTimeFormat.FormatProvider)}"/>
                  </C:comp-filter>
                </C:comp-filter>
              </C:filter>
            </C:calendar-query>
            """;
    }
}
