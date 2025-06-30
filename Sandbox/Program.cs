using CalDavNet;

using Microsoft.Extensions.DependencyInjection;

namespace Sandbox;

class Program
{
    static string Username = null!;
    static string Password = null!;

    static async Task Main(string[] args)
    {
        //DotNetEnv.Env.Load();

        //Username = Environment.GetEnvironmentVariable("YANDEX_USERNAME")!;
        //Password = Environment.GetEnvironmentVariable("YANDEX_PASSWORD")!;

        //var services = new ServiceCollection();

        //services.AddCalDavClient(options =>
        //{
        //    options.BaseAddress = new Uri("https://caldav.yandex.ru");
        //});

        //var provider = services.BuildServiceProvider();

        //using var scope = provider.CreateScope();
        //var calDavClient = scope.ServiceProvider.GetRequiredService<CalDavClient>();
        //var client = new Client(calDavClient, Username, Password);

        //await Process(client);

        #region Filter Test

        FilterBuilder filterBuilder = new FilterBuilder();

        filterBuilder.AddCompFilter(Constants.CompFilter.VEVENT)
            .AddTimeRange(DateTime.UtcNow, DateTime.UtcNow.AddMonths(1))
            .AddTextMatch(Constants.PropFilter.SUMMARY, "Meeting", true, "i;unicode-casemap");

        Console.WriteLine(filterBuilder.ToXElement.ToString());

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

        var calendars = await client.GetCalendarsAsync(principal.CalendarHomeSet,
            BodyBuilder.BuildPropfindBody([], [XNames.ResourceType, XNames.GetCTag, XNames.SyncToken]));

        ArgumentNullException.ThrowIfNull(calendars);
        
        Console.WriteLine("\nCalendars list:");

        foreach (var calendar in calendars)
        {
            Console.WriteLine();
            Console.WriteLine(calendar.Uri);
        }

        ArgumentNullException.ThrowIfNull(calendars.FirstOrDefault());

        var defaultCalendar = await client.GetCalendarByUriAsync(calendars.FirstOrDefault()!.Uri,
            BodyBuilder.BuildPropfindBody([XNames.AllProp], []));

        ArgumentNullException.ThrowIfNull(defaultCalendar);

        Console.WriteLine("\nEstimated default calendar: " + defaultCalendar.DisplayName);

        var events = await client.GetEventsAsync(defaultCalendar.Uri,
            BodyBuilder.BuildReportBody(DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddMonths(1)));

        ArgumentNullException.ThrowIfNull(events);

        Console.WriteLine("\nEvents list: ");

        foreach (var @event in events)
        {
            Console.WriteLine();
            Console.WriteLine(@event.Uri);
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
