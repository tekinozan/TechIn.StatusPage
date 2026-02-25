using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TechIn.StatusPage.UI.Data;

namespace TechIn.StatusPage.UI.Storage.PostgreSQL;

public class StatusPagePostgresDbContextFactory : IDesignTimeDbContextFactory<StatusPageDbContext>
{
    public StatusPageDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<StatusPageDbContext>()
            .UseNpgsql("Host=localhost;Database=statuspage_design;Username=postgres;Password=postgres",
                    b => b.MigrationsAssembly("TechIn.StatusPage.UI.Storage.PostgreSQL"))
            .Options;

        return new StatusPageDbContext(options);
    }
}