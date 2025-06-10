using System.Net.Http.Headers;

using CalDavNet;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static CalDavBuilder AddCalDavClient(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        var builder = services
            .LastOrDefault(x => x.ServiceType == typeof(CalDavBuilder))
            ?.ImplementationInstance as CalDavBuilder;

        if (builder is null)
        {
            builder = new CalDavBuilder(services);
            services.AddSingleton(builder);
        }

        services.AddHttpClient(nameof(CalDavClient),
            options =>
            {
                ArgumentNullException.ThrowIfNull(builder.BaseAddress);
                options.BaseAddress = builder.BaseAddress;

                if (!string.IsNullOrEmpty(builder.Depth))
                    options.DefaultRequestHeaders.Add("Depth", builder.Depth);

                if (!string.IsNullOrEmpty(builder.Prefer))
                    options.DefaultRequestHeaders.Add("Prefer", builder.Prefer);

                options.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            });

        services.AddSingleton<CalDavClient>();

        return builder;
    }

    public static CalDavBuilder AddCalDavClient(this IServiceCollection services, Action<CalDavBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var builder = services.AddCalDavClient();

        configure(builder);

        return builder;
    }
}
