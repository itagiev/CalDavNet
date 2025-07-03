using CalDavNet;

using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;

using Microsoft.Extensions.DependencyInjection;

namespace Sandbox;

class Program
{
    static string Username = null!;
    static string Password = null!;

    static async Task Main(string[] args)
    {
        DotNetEnv.Env.Load();

        Username = Environment.GetEnvironmentVariable("YANDEX_USERNAME")!;
        Password = Environment.GetEnvironmentVariable("YANDEX_PASSWORD")!;

        var services = new ServiceCollection();

        services.AddCalDav(options =>
        {
            options.BaseAddress = new Uri("https://caldav.yandex.ru");
        });

        var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var calDavClient = scope.ServiceProvider.GetRequiredService<CalDavClient>();
        var client = new Client(calDavClient, Username, Password);

        await Process(client);

        #region Filter Test

        //FilterBuilder filterBuilder = new FilterBuilder();

        //filterBuilder.AddCompFilter(Constants.CompFilter.VEVENT)
        //    .AddTimeRange(DateTime.UtcNow, DateTime.UtcNow.AddMonths(1))
        //    .AddTextMatch(Constants.PropFilter.SUMMARY, "Meeting", false, "i;unicode-casemap");

        //Console.WriteLine(filterBuilder.ToXElement.ToString());

        #endregion
    }

    static async Task Process(Client client)
    {
        var upn = await client.GetPrincipalNameAsync();

        ArgumentNullException.ThrowIfNull(upn);

        Console.WriteLine("User principal name: " + upn);
        var principal = await client.GetPrincipalAsync(upn);

        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(principal.CalendarHomeSet);

        Console.WriteLine("\nCalendar home set: " + principal.CalendarHomeSet);

        var vcalendars = await client.GetCalendarsAsync(principal.CalendarHomeSet,
            BuildBodyHelper.BuildPropfindBody([], [XNames.ResourceType, XNames.GetCtag, XNames.SyncToken]));

        ArgumentNullException.ThrowIfNull(vcalendars);
        
        Console.WriteLine("\nCalendars list:");

        foreach (var vcal in vcalendars)
        {
            Console.WriteLine(vcal.Uri);
        }

        ArgumentNullException.ThrowIfNull(vcalendars.FirstOrDefault());

        var defaultVCalendar = await client.GetCalendarByUriAsync(vcalendars.FirstOrDefault()!.Uri,
            BuildBodyHelper.BuildPropfindBody([XNames.AllProp], []));

        ArgumentNullException.ThrowIfNull(defaultVCalendar);

        Console.WriteLine("\nEstimated default calendar: " + defaultVCalendar.DisplayName);

        var filter = new FilterBuilder()
            .AddCompFilter(new CompFilterBuilder(Constants.CompFilter.VEVENT)
                .AddTimeRange(DateTime.UtcNow, DateTime.UtcNow.AddMonths(1))
                .ToXElement);

        var vevents = await client.GetEventsAsync(defaultVCalendar.Uri,
            BuildBodyHelper.BuildCalendarQueryBody(filter.ToXElement));

        ArgumentNullException.ThrowIfNull(vevents);

        Console.WriteLine("\nEvents uri list: ");

        foreach (var vevent in vevents)
        {
            Console.WriteLine(vevent.Uri);
        }

        {
            var loadedVEvents = await client.GetEventsAsync(defaultVCalendar.Uri,
                BuildBodyHelper.BuildCalendarMultigetBody(vevents.Select(x => x.Uri)));

            ArgumentNullException.ThrowIfNull(loadedVEvents);

            Console.WriteLine("\nLoaded events by uri: ");
            Ical.Net.Calendar? calendar = null;

            foreach (var vevent in loadedVEvents)
            {
                if (calendar is null)
                {
                    calendar = Ical.Net.Calendar.Load(vevent.CalendarData);
                }
                else
                {
                    calendar.MergeWith(Ical.Net.Calendar.Load(vevent.CalendarData));
                }
            }

            foreach (var e in calendar?.Events ?? Enumerable.Empty<CalendarEvent>())
            {
                Console.WriteLine($"Summary: {e.Summary}");
            }
        }

        // Event creation
        {
            Console.WriteLine();

            //var uid = Guid.NewGuid().ToString();
            var uid = "145629b9-7238-4070-af93-32c275b9dd3d";

            var date = new CalDateTime(DateTime.UtcNow.AddHours(12));
            var @event = new CalendarEvent
            {
                // If Name property is used, it MUST be RFC 5545 compliant
                Summary = "Spider-man", // Should always be present
                Description = "Hobbit goes here", // optional
                Start = date,
                End = date.AddMinutes(45),
                Uid = uid
            };

            var calendar = new Ical.Net.Calendar();
            calendar.AddTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time"));
            calendar.Events.Add(@event);

            var serializer = new CalendarSerializer();
            var body = serializer.SerializeToString(calendar);

            await client.CreateEventAsync(defaultVCalendar.Uri, uid, body);

            var loadedEvent = await client.GetEventAsync(defaultVCalendar.Uri, $"{defaultVCalendar.Uri}{uid}.ics");

            //if (loadedEvent != null)
            //    await loadedEvent.Delete(client);
        }
    }

    //static async Task TestCalendarsAsync(Client client)
    //{
    //    var calendars = await client.GetCalendarsAsync((CancellationToken)default, XNames.ResourceType, XNames.DisplayName,
    //        XNames.GetCTag, XNames.SupportedCalendarComponentSet, XNames.GetLastModified);

    //    foreach (var calendar in calendars ?? [])
    //    {
    //        Console.WriteLine($"{calendar.DisplayName} {calendar.Uri}");
    //    }
    //}

    //static async Task TestCalendarAsync(Client client)
    //{
    //    var body = Helpers.BuildPropfindBody(XNames.CalendarData);

    //    var token = GetAuthToken();

    //    var response = await client.GetCalendarAsync(token, @"/calendars/i.tagiev%40adamcode.ru/events-27560559/", body);

    //    var entry = response.Entries.FirstOrDefault();

    //    Console.WriteLine(entry!.Properties[XNames.CalendarData].Value);
    //}

    //static async Task AllCalendarsTestAsync(Client client)
    //{
    //    var calendars = await client.GetCalendarsAsync((CancellationToken)default, XNames.ResourceType, XNames.DisplayName,
    //        XNames.GetCTag, XNames.SupportedCalendarComponentSet, XNames.GetLastModified);

    //    foreach (var calendar in calendars ?? [])
    //    {
    //        Console.WriteLine("====================================================");
    //        Console.WriteLine($"{calendar.DisplayName ?? "[NO_DISPLAY_NAME]"} {calendar.Uri}");

    //        var c = await client.GetCalendarByUriAsync(calendar.Uri, Helpers.BuildAllPropPropfindBody());

    //        Console.WriteLine("====================================================");
    //    }
    //}
}
