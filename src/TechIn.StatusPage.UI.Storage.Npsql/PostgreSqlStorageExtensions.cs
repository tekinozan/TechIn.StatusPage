using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using TechIn.StatusPage.Core;
using TechIn.StatusPage.Core.Extensions;
using TechIn.StatusPage.UI.Data;

namespace TechIn.StatusPage.UI.Storage.PostgreSQL;

public static class PostgreSqlStorageExtensions
{
    public static StatusPageBuilder AddPostgreSqlStorage(
        this StatusPageBuilder builder,
        string connectionString,
        Action<NpgsqlDbContextOptionsBuilder>? configurePostgreOptions = null)
    {
        builder.Services.AddDbContextFactory<StatusPageDbContext>(options => 
        {
            options.UseNpgsql(connectionString, npgsqlOptionsBuilder =>
            {
                npgsqlOptionsBuilder.MigrationsAssembly(typeof(PostgreSqlStorageExtensions).Assembly.FullName);
                configurePostgreOptions?.Invoke(npgsqlOptionsBuilder);
            });
        });

        builder.Services.AddHostedService<DatabaseMigrationHostedService<StatusPageDbContext>>();
        builder.UseStatusRepository<EfCoreStatusRepository>(ServiceLifetime.Scoped);
        return builder;
    }
}
