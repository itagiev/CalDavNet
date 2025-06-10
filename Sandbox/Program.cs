using CalDavNet;

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

        services.AddCalDavClient(options =>
        {
            options.BaseAddress = new Uri("https://caldav.yandex.ru");
        });

        var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var calDavClient = scope.ServiceProvider.GetRequiredService<CalDavClient>();
        var client = new Client(calDavClient, Username, Password);

        await Process(client);
    }

    static async Task Process(Client client)
    {
        var upn = await client.GetPrincipalNameAsync();

        ArgumentNullException.ThrowIfNull(upn);

        Console.WriteLine(upn);
        var principal = await client.GetPrincipalAsync(upn);

        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(principal.CalendarHomeSet);

        Console.WriteLine(principal.CalendarHomeSet);

        var calendars = await client.GetCalendarsAsync(principal.CalendarHomeSet,
            [], [XNames.ResourceType, XNames.DisplayName, XNames.GetCTag, XNames.SyncToken]);

        ArgumentNullException.ThrowIfNull(calendars);

        foreach (var calendar in calendars)
        {
            Console.WriteLine("========================");
            Console.WriteLine(calendar.Uri);
            Console.WriteLine(calendar.DisplayName);
            Console.WriteLine(calendar.CTag);
            Console.WriteLine(calendar.SyncToken);
            Console.WriteLine("========================");

            var fullCalendar = await client.GetCalendarByUriAsync(calendar.Uri,
                [XNames.AllProp], []);
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
