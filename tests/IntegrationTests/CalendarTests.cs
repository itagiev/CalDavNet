using CalDavNet;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests;

public class CalendarTests : IAsyncLifetime
{
    private IServiceScope _scope = null!;
    private CalDavClient _calDavClient = null!;
    private Client _client = null!;
    private string _upn = null!;
    private Principal _principal = null!;
    private string _calendarHomeSet = null!;

    public async Task InitializeAsync()
    {
        DotNetEnv.Env.Load();

        var username = Environment.GetEnvironmentVariable("YANDEX_USERNAME")!;
        var password = Environment.GetEnvironmentVariable("YANDEX_PASSWORD")!;

        var services = new ServiceCollection();

        services.AddCalDav("yandex", options =>
        {
            options.BaseAddress = new Uri(Environment.GetEnvironmentVariable("YANDEX_CALDAV_URI")!);
        });

        var provider = services.BuildServiceProvider();

        _scope = provider.CreateScope();
        _calDavClient = _scope.ServiceProvider.GetRequiredKeyedService<CalDavClient>("yandex");
        _client = new Client(_calDavClient, username, password);

        _upn = (await _client.GetPrincipalNameAsync())!;

        ArgumentNullException.ThrowIfNull(_upn);

        _principal = (await _client.GetPrincipalAsync(_upn))!;
        _calendarHomeSet = _principal.CalendarHomeSet!;

        ArgumentNullException.ThrowIfNull(_principal);
        ArgumentNullException.ThrowIfNull(_principal.CalendarHomeSet);
    }

    public Task DisposeAsync()
    {
        _scope?.Dispose();

        return Task.CompletedTask;
    }

    [Fact]
    public async Task ShouldFindCalendars()
    {
        // Act
        var calendars = await _client.GetCalendarsAsync(_calendarHomeSet, BodyHelper.BuildPropfind(
            [XNames.ResourceType, XNames.GetCtag, XNames.SyncToken, XNames.SupportedCalendarComponentSet, XNames.DisplayName]));

        // Assert
        calendars.Count.Should().BeGreaterThan(0);
    }
}
