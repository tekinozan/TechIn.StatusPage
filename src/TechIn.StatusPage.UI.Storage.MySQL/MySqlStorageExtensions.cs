using Microsoft.Extensions.DependencyInjection;
using TechIn.StatusPage.Core;
using TechIn.StatusPage.UI.Data;
using Microsoft.EntityFrameworkCore;
using TechIn.StatusPage.Core.Extensions;
using MySql.EntityFrameworkCore.Infrastructure;

namespace TechIn.StatusPage.UI.Storage.MySQL;

public static class MySqlStorageExtensions
{
    public static StatusPageBuilder AddMySqlStorage(
        this StatusPageBuilder builder,
        string connectionString,
        Action<MySQLDbContextOptionsBuilder>? configureMySqlOptions = null)
    {
        builder.Services.AddDbContextFactory<StatusPageDbContext>(options =>
        {
            options.UseMySQL(connectionString, mysqlOptionsBuilder =>
            {
                mysqlOptionsBuilder.MigrationsAssembly(typeof(MySqlStorageExtensions).Assembly.FullName);
                configureMySqlOptions?.Invoke(mysqlOptionsBuilder);
            });
        });

        builder.Services.AddHostedService<DatabaseMigrationHostedService<StatusPageDbContext>>();
        builder.UseStatusRepository<EfCoreStatusRepository>(ServiceLifetime.Scoped);
        return builder;
    }
}
