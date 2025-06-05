using System.Text;

using Microsoft.Extensions.DependencyInjection;

using CalDavNet;
using Ical.Net;
using System.Xml;
using System.Xml.Linq;

namespace Sandbox;

class Program
{
    static async Task Main(string[] args)
    {
        var email = "i.tagiev@adamcode.ru";
        var token = GetAuthToken();
        var services = new ServiceCollection();
        services.AddYandexCal(options =>
        {
            options.BaseAddress = new Uri("https://caldav.yandex.ru");
        });

        var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var client = scope.ServiceProvider.GetRequiredService<CalDavClient>();


        var report = await client.ReportAsync(token, email);
    }

    static string GetAuthToken()
    {
        string username = "i.tagiev@adamcode.ru";
        string password = "kzjrzvxckbmettkj";
        string token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
        return token;
    }
}
