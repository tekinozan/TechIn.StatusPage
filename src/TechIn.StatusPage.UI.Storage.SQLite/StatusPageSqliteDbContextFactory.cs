using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TechIn.StatusPage.UI.Data;

namespace TechIn.StatusPage.UI.Storage.SQLite;

public class StatusPageSqliteDbContextFactory : IDesignTimeDbContextFactory<StatusPageDbContext>
{
    public StatusPageDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<StatusPageDbContext>()
            .UseSqlite("Data Source=statuspage_design.db",
                    b => b.MigrationsAssembly("TechIn.StatusPage.UI.Storage.SQLite"))
            .Options;

        return new StatusPageDbContext(options);
    }
}