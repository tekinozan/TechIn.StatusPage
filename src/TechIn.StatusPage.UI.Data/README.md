# TechIn.StatusPage.UI.Data

Shared data layer for [TechIn.StatusPage](https://github.com/techin/statuspage) storage providers. Contains the EF Core `DbContext`, entity configurations, and the common `EfCoreStatusRepository` used by database-backed providers.

> **Note**: This package is a dependency of the storage provider packages. You typically don't need to install it directly — it's pulled in automatically when you add a provider like `TechIn.StatusPage.UI.Storage.PostgreSQL`.

## Installation

```bash
dotnet add package TechIn.StatusPage.UI.Data
```

## What's Inside

| Component | Description |
|---|---|
| `StatusPageDbContext` | EF Core `DbContext` with `DbSet<HealthSnapshot>` |
| `HealthSnapshotConfiguration` | Entity configuration with snake_case column names and indexes |
| `EfCoreStatusRepository` | Shared `IStatusRepository` implementation for relational databases |
| `DatabaseMigrationHostedService<TContext>` | Generic hosted service that applies EF Core migrations at startup |

## Schema

The `HealthSnapshotConfiguration` defines the following table structure:

```
snapshots
├── id              bigint (PK, auto-increment)
├── service_name    varchar(256)
├── status          varchar(32)     — Operational, Degraded, Down
├── timestamp       datetimeoffset
├── latency         nullable
└── description     varchar(2048), nullable
```

### Indexes

- `ix_snapshots_service_name`
- `ix_snapshots_service_name_timestamp`
- `ix_snapshots_timestamp`

### Schema Prefix

PostgreSQL and SQL Server providers use the `status_page` schema. Providers that don't support schemas (MySQL, SQLite) store tables without a prefix — this is handled automatically in `StatusPageDbContext.OnModelCreating`.

## Building a Custom Storage Provider

If you're building your own storage provider package, reference this package and use the shared components:

```csharp
using TechIn.StatusPage.UI.Data;

public static class MyProviderExtensions
{
    public static StatusPageBuilder AddMyProvider(
        this StatusPageBuilder builder,
        string connectionString)
    {
        // Register DbContextFactory with your provider
        builder.Services.AddDbContextFactory<StatusPageDbContext>(options =>
            options.UseYourProvider(connectionString, opt =>
                opt.MigrationsAssembly(typeof(MyProviderExtensions).Assembly.FullName)));

        // Use the shared EfCoreStatusRepository or your own
        builder.Services.AddHostedService<DatabaseMigrationHostedService<StatusPageDbContext>>();
        builder.UseStatusRepository<EfCoreStatusRepository>(ServiceLifetime.Scoped);
        return builder;
    }
}
```

If your provider has LINQ translation limitations (like SQLite with `DateTimeOffset`), implement a custom `IStatusRepository` instead of using `EfCoreStatusRepository`.

## Requirements

- .NET 8.0, 9.0, or 10.0

## License

MIT — see [LICENSE](LICENSE) for details.