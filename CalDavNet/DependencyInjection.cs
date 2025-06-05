using CalDavNet;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static CalDavBuilder AddYandexCal(this IServiceCollection services)
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
                options.BaseAddress = builder.BaseAddress;
                options.DefaultRequestHeaders.Add("Depth", "1");
            });

        services.AddSingleton<CalDavClient>();

        return builder;
    }

    public static CalDavBuilder AddYandexCal(this IServiceCollection services, Action<CalDavBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var builder = services.AddYandexCal();

        configure(builder);

        return builder;
    }
}
