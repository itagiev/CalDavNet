using Microsoft.Extensions.DependencyInjection;

namespace CalDavNet;

public class CalDavBuilder
{
    public IServiceCollection Services { get; }

    public Uri BaseAddress { get; set; } = null!;

    public CalDavBuilder(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        Services = services;
    }
}
