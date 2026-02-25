using Microsoft.Extensions.DependencyInjection;

namespace TechIn.StatusPage.Core;

public class StatusPageBuilder
{
    public IServiceCollection Services { get; }

    public StatusPageBuilder(IServiceCollection services)
    {
        Services = services;
    }
}
