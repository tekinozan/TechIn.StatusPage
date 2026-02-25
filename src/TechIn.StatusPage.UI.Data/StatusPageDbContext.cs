using Microsoft.EntityFrameworkCore;
using TechIn.StatusPage.Core.Models;
using TechIn.StatusPage.UI.Data.Configurations;

namespace TechIn.StatusPage.UI.Data;

public sealed class StatusPageDbContext(DbContextOptions<StatusPageDbContext> options) : DbContext(options)
{
    public DbSet<HealthSnapshot> Snapshots { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("status_page");
        modelBuilder.ApplyConfiguration(new HealthSnapshotConfiguration());
    }
}
