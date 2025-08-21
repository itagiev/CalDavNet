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

        services.AddCalDav("yandex", options =>
        {
            options.BaseAddress = new Uri("https://caldav.yandex.ru");
            options.EnableLoggingHandler = false;
        });

        var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var calDavClient = scope.ServiceProvider.GetRequiredKeyedService<CalDavClient>("yandex");
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
            BodyHelper.BuildPropfind([],
                [XNames.ResourceType, XNames.GetCtag, XNames.SyncToken, XNames.SupportedCalendarComponentSet, XNames.DisplayName]));

        Console.WriteLine();
        if (calendars.Count > 0)
        {
            Console.WriteLine("Calendars list:");

            foreach (var calendar in calendars)
            {
                Console.WriteLine($"{calendar.DisplayName} - {calendar.Href}");
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
                BodyHelper.BuildPropfind([XNames.ResourceType, XNames.GetCtag, XNames.SyncToken, XNames.DisplayName]));

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
            throw;
        }

        // TEST: Sync
        //var syncItemCollection = await defaultCalendar.SyncItemsAsync(client, null);

        //foreach (var e in syncItemCollection)
        //{
        //    Console.WriteLine($"{e.Href} - {e.Etag}");
        //}

        Console.WriteLine();
        Console.WriteLine("-----------------------------------------");
        Console.WriteLine("Processing mailbox logic\n");
        await ProcessMailboxLogic(client, principal.CalendarHomeSet);

        //Console.WriteLine();
        //Console.WriteLine("-----------------------------------------");
        //Console.WriteLine("Processing calendar logic\n");
        //await ProcessCalendarLogic(client, defaultCalendar);

        //Console.WriteLine();
        //Console.WriteLine("-----------------------------------------");
        //Console.WriteLine("Processing event logic\n");
        //await ProcessEventLogic(client, defaultCalendar);
    }

    static async Task ProcessMailboxLogic(Client client, string calendarHomeSet)
    {
        string calendarName = "Haha calendar";
        var body = BodyHelper.BuildMkcalendar(calendarName, Constants.Comp.VEVENT, Guid.NewGuid().ToString());
        var result = await client.CreateCalendarAsync(calendarHomeSet, body);
        Console.WriteLine(result);

        var calendars = await client.GetCalendarsAsync(calendarHomeSet, BodyHelper.BuildPropfind(
            [XNames.ResourceType, XNames.SupportedCalendarComponentSet, XNames.GetCtag, XNames.DisplayName, XNames.Comment]));

        Calendar myCalendar = null!;

        foreach (var calendar in calendars)
        {
            if (string.Equals(calendar.DisplayName, calendarName, StringComparison.OrdinalIgnoreCase))
            {
                myCalendar = calendar;
                break;
            }
        }

        // Updating
        result = await myCalendar.UpdateAsync(client, BodyHelper.BuildPropertyUpdate("Super new name"));
        Console.WriteLine(result);

        // Deleting
        result = await myCalendar.DeleteAsync(client);
        Console.WriteLine(result);
    }

    static async Task ProcessCalendarLogic(Client client, Calendar calendar)
    {
        // TEST: Getting filtered events
        var filter = new FilterBuilder()
            .AddCompFilter(new CompFilterBuilder(Constants.Comp.VEVENT)
                .AddTimeRange(DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow.AddMonths(1))
                .ToXElement);

        var events = await calendar.GetEventsAsync(client, BodyHelper.BuildCalendarQuery(filter.ToXElement));

        if (events.Count > 0)
        {
            Console.WriteLine("Event collection (loaded with calendar query body):");

            foreach (var @event in events)
            {
                Console.WriteLine(@event.Href);
            }

            Console.WriteLine();

            events = await calendar.GetEventsAsync(client, BodyHelper.BuildCalendarMultiget(events.Select(x => x.Href)));

            if (events.Count > 0)
            {
                Console.WriteLine("Event collection (loaded with multiget body):");

                foreach (var @event in events)
                {
                    if (@event.ICalCalendar is not null)
                    {
                        Console.Write($"Events count: {@event.ICalCalendar.Events.Count}");

                        if (@event.ICalEvent is not null)
                        {
                            Console.Write($" Summary: {@event.ICalEvent.Summary}");
                        }

                        Console.WriteLine();
                    }
                }
            }
            else
            {
                Console.WriteLine("Calendar has no events");
            }
        }
        else
        {
            Console.WriteLine("Calendar is empty");
        }
    }

    static async Task ProcessEventLogic(Client client, Calendar calendar)
    {
        // TEST: Getting events with calendar query body
        var date = new CalDateTime(DateTime.UtcNow.AddHours(1));
        var calendarEvent = new CalendarEvent
        {
            // If Name property is used, it MUST be RFC 5545 compliant
            Summary = "Spider-Man", // Should always be present
            Description = "Hobbit goes here", // optional
            Start = date,
            End = date.AddMinutes(45),
            Uid = Guid.NewGuid().ToString()
        };

        Console.WriteLine("Creating event...");

        var @event = await calendar.CreateEventAsync(client, calendarEvent);

        if (@event is not null)
        {
            Console.WriteLine($"Event created");
        }
        else
        {
            Console.WriteLine("Error occurred while creating event");
        }

        ArgumentNullException.ThrowIfNull(@event?.ICalEvent);

        Console.WriteLine($"{@event.ICalEvent.Summary} loaded");

        Console.WriteLine($"Event {@event.ICalEvent.Summary} created");

        Console.WriteLine("Updating event...");

        @event.ICalEvent.Summary = "New summary";
        var result = await @event.UpdateAsync(client);

        if (result)
        {
            Console.WriteLine($"Event {@event.ICalEvent.Summary} updated");
        }
        else
        {
            Console.WriteLine("Error occurred while updating event");
        }

        Console.WriteLine("Deleting event...");

        result = await @event.DeleteAsync(client);

        if (result)
        {
            Console.WriteLine("Event deleted");
        }
        else
        {
            Console.WriteLine("Error occurred while deleting event");
        }
    }
}
