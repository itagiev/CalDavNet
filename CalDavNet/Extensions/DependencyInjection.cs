using System.Net.Http.Headers;

using CalDavNet;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddCalDav(this IServiceCollection services, string clientName, Action<CalDavOptions> configure)
    {
        var options = new CalDavOptions();
        configure(options);

        var httpClientBuilder = services.AddHttpClient(clientName,
            client =>
            {
                ArgumentNullException.ThrowIfNull(options.BaseAddress);
                client.BaseAddress = options.BaseAddress;
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            });

        if (options.EnableLoggingHandler)
        {
            httpClientBuilder.AddHttpMessageHandler<LoggingHandler>();
            services.AddTransient<LoggingHandler>();
        }

        services.AddKeyedSingleton(clientName, (serviceProvider, _) =>
        {
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            return new CalDavClient(httpClientFactory, clientName);
        });
    }
}
