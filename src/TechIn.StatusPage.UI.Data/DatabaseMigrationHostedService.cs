using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TechIn.StatusPage.UI.Data;

public sealed class DatabaseMigrationHostedService<TContext> : IHostedService
    where TContext : DbContext
{
    private readonly IServiceProvider _serviceProvider;

    public DatabaseMigrationHostedService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<TContext>>();
        await using var context = await factory.CreateDbContextAsync(cancellationToken);

        await context.Database.MigrateAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}