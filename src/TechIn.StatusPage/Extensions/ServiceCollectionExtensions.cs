using Microsoft.Extensions.DependencyInjection;
using TechIn.StatusPage.Core;
using TechIn.StatusPage.Core.Interfaces;
using TechIn.StatusPage.Core.Models;
using TechIn.StatusPage.Services;

namespace TechIn.StatusPage.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all services required for the status page.
    /// Call this in your <c>Program.cs</c> before building the app.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration callback for <see cref="StatusPageOptions"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static StatusPageBuilder AddStatusPage(
        this IServiceCollection services,
        Action<StatusPageOptions>? configure = null)
    {
        // Register options
        if (configure is not null)
            services.Configure(configure);
        else
            services.Configure<StatusPageOptions>(_ => { });

        // Default in-memory repository
        services.AddSingleton<IStatusRepository, InMemoryStatusRepository>();

        services.AddScoped<IStatusPageService, DefaultStatusPageService>();
        services.AddHostedService<HealthHistoryCollector>();

        return new StatusPageBuilder(services);
    }
}