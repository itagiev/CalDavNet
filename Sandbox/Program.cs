using CalDavNet;

using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;

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
        // TEST: Getting UPN
        var upn = await client.GetPrincipalNameAsync();

        ArgumentNullException.ThrowIfNull(upn);

        Console.WriteLine($"User principal name {upn}");

        // TEST: Getting principal data
        var principal = await client.GetPrincipalAsync(upn);

        ArgumentNullException.ThrowIfNull(principal);

        Console.WriteLine();
        Console.WriteLine($"Principal {principal.DisplayName ?? "[NO DISPLAY NAME]"}");
        Console.WriteLine($"Calendar home set {principal.CalendarHomeSet ?? "[NO CALENDAR HOME SET]"}");

        ArgumentNullException.ThrowIfNull(principal.CalendarHomeSet);

        // TEST: Getting calendars
        var calendars = await client.GetCalendarsAsync(principal.CalendarHomeSet,
            BuildBodyHelper.BuildPropfindBody([], [XNames.ResourceType, XNames.GetCtag, XNames.SyncToken]));

        Console.WriteLine();
        if (calendars.Count > 0)
        {
            Console.WriteLine("Calendars list:");

            foreach (var calendar in calendars)
            {
                Console.WriteLine(calendar.Href);
            }
        }
        else
        {
            Console.WriteLine("Calendars list is empty");
        }

        // TEST: Getting default calendar
        var defaultCalendar = calendars.FirstOrDefault();
        ArgumentNullException.ThrowIfNull(defaultCalendar);

        Console.WriteLine();
        Console.WriteLine($"Estimated default calendar {defaultCalendar.Href}");

        // TEST: Getting single calendar by it's href
        try
        {
            Console.WriteLine();
            Console.WriteLine($"Loading single calendar {defaultCalendar.Href}");

            defaultCalendar = await client.GetCalendarAsync(calendars.FirstOrDefault()!.Href,
                BuildBodyHelper.BuildPropfindBody([XNames.ResourceType, XNames.GetCtag, XNames.SyncToken, XNames.DisplayName]));

            /*
             * Or
             * 
             * defaultCalendar = await client.GetCalendarAsync(calendars.FirstOrDefault()!.Href,
                BuildBodyHelper.BuildPropfindBody([XNames.AllProp], []));
             * Or
             * 
             * defaultCalendar = await client.GetCalendarAsync(calendars.FirstOrDefault()!.Href,
                BuildBodyHelper.BuildAllPropPropfindBody());
             */

            ArgumentNullException.ThrowIfNull(defaultCalendar);

            Console.WriteLine($"Calendar {defaultCalendar.DisplayName} loaded successfully");
        }
        catch
        {
            Console.WriteLine($"Calendar not found");
        }

        return;

        var filter = new FilterBuilder()
            .AddCompFilter(new CompFilterBuilder(Constants.CompFilter.VEVENT)
                .AddTimeRange(DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow.AddMonths(1))
                .ToXElement);

        var vevents = await client.GetEventsAsync(defaultCalendar.Href,
            BuildBodyHelper.BuildCalendarQueryBody(filter.ToXElement));

        ArgumentNullException.ThrowIfNull(vevents);

        Console.WriteLine("\nEvents href list: ");

        foreach (var vevent in vevents)
        {
            Console.WriteLine(vevent.Href);
        }

        {
            var loadedVEvents = await client.GetEventsAsync(defaultCalendar.Href,
                BuildBodyHelper.BuildCalendarMultigetBody(vevents.Select(x => x.Href)));

            ArgumentNullException.ThrowIfNull(loadedVEvents);

            Console.WriteLine("\nLoaded events by href: ");
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

        Console.WriteLine("\n====================================");
        Console.WriteLine("Processing entity logic\n");
        //await ProcessEntityLogic(client, defaultCalendar);
    }

    static async Task ProcessEntityLogic(Client client, Calendar defaultCalendar,
        CancellationToken cancellationToken = default)
    {
        var date = new CalDateTime(DateTime.UtcNow.AddHours(2));
        var calendarEvent = new CalendarEvent
        {
            // If Name property is used, it MUST be RFC 5545 compliant
            Summary = "Spider-man", // Should always be present
            Description = "Hobbit goes here", // optional
            Start = date,
            End = date.AddMinutes(45),
            Uid = Guid.NewGuid().ToString()
        };

        var @event = await defaultCalendar.CreateEventAsync(client, calendarEvent);

        ArgumentNullException.ThrowIfNull(@event);

        var loadedEvent = await defaultCalendar.GetEventAsync(client, @event.Href);

        //if (loadedEvent != null)
        //    await loadedEvent.Delete(client);
    }
}
