using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using TechIn.StatusPage.Core;
using TechIn.StatusPage.Core.Extensions;
using TechIn.StatusPage.UI.Data;

namespace TechIn.StatusPage.UI.Storage.SQLite;

public static class SqliteExtensions
{
    public static StatusPageBuilder AddSqliteStorage(
       this StatusPageBuilder builder,
       string connectionString,
       Action<SqliteDbContextOptionsBuilder>? configureSqliteOptions = null)
    {
        builder.Services.AddDbContextFactory<StatusPageDbContext>(options =>
        {
            options.UseSqlite(connectionString, sqliteOptionsBuilder =>
            {
                sqliteOptionsBuilder.MigrationsAssembly(typeof(SqliteExtensions).Assembly.FullName);
                configureSqliteOptions?.Invoke(sqliteOptionsBuilder);
            });
        });

        builder.Services.AddHostedService<DatabaseMigrationHostedService<StatusPageDbContext>>();
        builder.UseStatusRepository<SqliteStatusRepository>(ServiceLifetime.Scoped);
        return builder;
    }

}
