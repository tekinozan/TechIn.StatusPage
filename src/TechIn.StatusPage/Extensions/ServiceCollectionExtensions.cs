using Microsoft.Extensions.DependencyInjection;
using TechIn.StatusPage.Core.Interfaces;
using TechIn.StatusPage.Core.Models;
using TechIn.StatusPage.Services;

namespace TechIn.StatusPage.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all services required for the Google-style status page.
    /// Call this in your <c>Program.cs</c> before building the app.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration callback for <see cref="StatusPageOptions"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddStatusPage(
        this IServiceCollection services,
        Action<StatusPageOptions>? configure = null)
    {
        // Register options
        if (configure is not null)
            services.Configure(configure);
        else
            services.Configure<StatusPageOptions>(_ => { });

        // Register default in-memory repository (can be overridden by the consumer)
        services.AddSingleton<IStatusRepository, InMemoryStatusRepository>();

        // Register the service that assembles the response
        services.AddScoped<IStatusPageService, DefaultStatusPageService>();

        // Register the background collector
        services.AddHostedService<HealthHistoryCollector>();

        return services;
    }

    /// <summary>
    /// Replaces the default in-memory repository with a custom implementation.
    /// Call this AFTER <see cref="AddStatusPage"/>.
    /// </summary>
    public static IServiceCollection UseStatusRepository<TRepository>(this IServiceCollection services)
        where TRepository : class, IStatusRepository
    {
        // Remove existing registrations
        var existing = services.FirstOrDefault(d => d.ServiceType == typeof(IStatusRepository));
        if (existing is not null)
            services.Remove(existing);

        services.AddSingleton<IStatusRepository, TRepository>();
        return services;
    }
}