using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TechIn.StatusPage.UI.Data;

namespace TechIn.StatusPage.UI.Storage.MySQL;

public class StatusPageMySqlDbContextFactory : IDesignTimeDbContextFactory<StatusPageDbContext>
{
    public StatusPageDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<StatusPageDbContext>()
            .UseMySQL("Server=localhost;Database=statuspage_design;User=root;Password=root",
                    b => b.MigrationsAssembly("TechIn.StatusPage.UI.Storage.MySQL"))
            .Options;

        return new StatusPageDbContext(options);
    }
}