using Microsoft.Extensions.DependencyInjection;
using TechIn.StatusPage.Core.Interfaces;

namespace TechIn.StatusPage.Core.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Replaces the default in-memory repository with a custom implementation.
    /// Call this AFTER <see cref="AddStatusPage"/>.
    /// </summary>
    public static StatusPageBuilder UseStatusRepository<TRepository>(
        this StatusPageBuilder builder,
        ServiceLifetime serviceLifetime = ServiceLifetime.Singleton)
        where TRepository : class, IStatusRepository
    {
        // Remove existing registrations
        var existing = builder.Services.FirstOrDefault(d => d.ServiceType == typeof(IStatusRepository));
        if (existing is not null)
            builder.Services.Remove(existing);

        builder.Services.Add(new ServiceDescriptor(typeof(IStatusRepository), typeof(TRepository), serviceLifetime));
        return builder;
    }
}