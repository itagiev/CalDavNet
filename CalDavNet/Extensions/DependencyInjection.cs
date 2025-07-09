using System.Net.Http.Headers;

using CalDavNet;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddCalDav(this IServiceCollection services, string clientName, Action<CalDavOptions> configure)
    {
        var options = new CalDavOptions();
        configure(options);

        services.AddHttpClient(clientName,
            client =>
            {
                ArgumentNullException.ThrowIfNull(options.BaseAddress);
                client.BaseAddress = options.BaseAddress;

                if (!string.IsNullOrEmpty(options.Depth))
                    client.DefaultRequestHeaders.Add("Depth", options.Depth);

                if (!string.IsNullOrEmpty(options.Prefer))
                    client.DefaultRequestHeaders.Add("Prefer", options.Prefer);

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            });

        services.AddKeyedSingleton(clientName, (serviceProvider, _) =>
        {
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            return new CalDavClient(httpClientFactory, clientName);
        });
    }
}
