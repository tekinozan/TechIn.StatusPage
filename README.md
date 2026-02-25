# TechIn.StatusPage Storage Providers

Storage providers for [TechIn.StatusPage](https://github.com/techin/statuspage) that persist health-check snapshots to a database. Each provider plugs in through the `StatusPageBuilder` API — pick one, add a connection string, and you're done.

## Available Providers

| Package                                   | Database       | NuGet                                                                                                                                                  |
| ----------------------------------------- | -------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------ |
| `TechIn.StatusPage.UI.Storage.PostgreSQL` | PostgreSQL 12+ | [![NuGet](https://img.shields.io/nuget/v/TechIn.StatusPage.UI.Storage.PostgreSQL)](https://nuget.org/packages/TechIn.StatusPage.UI.Storage.PostgreSQL) |
| `TechIn.StatusPage.UI.Storage.MySQL`      | MySQL 8+       | [![NuGet](https://img.shields.io/nuget/v/TechIn.StatusPage.UI.Storage.MySQL)](https://nuget.org/packages/TechIn.StatusPage.UI.Storage.MySQL)           |
| `TechIn.StatusPage.UI.Storage.Sqlite`     | SQLite 3       | [![NuGet](https://img.shields.io/nuget/v/TechIn.StatusPage.UI.Storage.Sqlite)](https://nuget.org/packages/TechIn.StatusPage.UI.Storage.Sqlite)         |

## Quick Start

### PostgreSQL

```bash
dotnet add package TechIn.StatusPage.UI.Storage.PostgreSQL
```

```csharp
builder.Services
    .AddStatusPage(o => o.HistoryRetentionDays = 90)
    .AddPostgreSqlStorage(builder.Configuration.GetConnectionString("StatusPage")!);
```

### MySQL

```bash
dotnet add package TechIn.StatusPage.UI.Storage.MySQL
```

```csharp
builder.Services
    .AddStatusPage(o => o.HistoryRetentionDays = 90)
    .AddMySqlStorage(builder.Configuration.GetConnectionString("StatusPage")!);
```

### SQLite

```bash
dotnet add package TechIn.StatusPage.UI.Storage.Sqlite
```

```csharp
builder.Services
    .AddStatusPage(o => o.HistoryRetentionDays = 90)
    .AddSqliteStorage("Data Source=statuspage.db");
```

## How It Works

Each provider registers three things behind the scenes:

1. **DbContextFactory** — configured for the chosen database engine.
2. **IStatusRepository** — the implementation that reads/writes health snapshots.
3. **DatabaseMigrationHostedService** — automatically applies EF Core migrations when the application starts. No manual `dotnet ef database update` required in production.

The base `AddStatusPage()` call registers the health-check collector, the status page service, and the default in-memory repository. The storage extension replaces the in-memory repository with a persistent one.

## Database Schema

All providers create a `snapshots` table with the following columns:

| Column         | Type             | Description                             |
| -------------- | ---------------- | --------------------------------------- |
| `id`           | `bigint` (auto)  | Primary key                             |
| `service_name` | `varchar(256)`   | Name of the monitored service           |
| `status`       | `varchar(32)`    | `Operational`, `Degraded`, or `Down`    |
| `timestamp`    | `datetimeoffset` | UTC timestamp of the health check       |
| `latency`      | `timespan`       | Response time (nullable)                |
| `description`  | `varchar(2048)`  | Error message or description (nullable) |

PostgreSQL and SQL Server use the `status_page` schema. MySQL and SQLite store tables without a schema prefix.

### Indexes

- `ix_snapshots_service_name` — filters by service
- `ix_snapshots_service_name_timestamp` — date-range queries per service
- `ix_snapshots_timestamp` — purge operations

## Migrations

Migrations are bundled inside each provider package and applied automatically at startup. If you need to apply them manually:

```bash
# PostgreSQL
dotnet ef database update \
  --project src/TechIn.StatusPage.UI.Storage.PostgreSQL \
  --framework net8.0

# MySQL
dotnet ef database update \
  --project src/TechIn.StatusPage.UI.Storage.MySQL \
  --framework net8.0

# SQLite
dotnet ef database update \
  --project src/TechIn.StatusPage.UI.Storage.Sqlite \
  --framework net8.0
```

## Configuration

All providers respect the `StatusPageOptions` configured via `AddStatusPage()`:

```csharp
builder.Services.AddStatusPage(options =>
{
    // How many days of snapshot history to keep (default: 90)
    options.HistoryRetentionDays = 60;

    // Polling interval for health checks (default: 30 seconds)
    options.PollingInterval = TimeSpan.FromSeconds(15);

    // Filter which health checks to track
    options.HealthCheckFilter = name => name != "self";
});
```

## Connection String Examples

### PostgreSQL

```
Host=localhost;Port=5432;Database=statuspage;Username=app;Password=secret;
```

### MySQL

```
Server=localhost;Port=3306;Database=statuspage;User=app;Password=secret;
```

### SQLite

```
Data Source=statuspage.db
```

## Custom Repository

If the built-in providers don't fit your needs, you can implement `IStatusRepository` and register it with `UseStatusRepository<T>()`:

```csharp
public class RedisStatusRepository : IStatusRepository
{
    // Your custom implementation
    public Task SaveSnapshotsAsync(IEnumerable<HealthSnapshot> snapshots, CancellationToken ct = default) { ... }
    public Task<IReadOnlyList<DayAggregate>> GetDailyAggregatesAsync(string serviceName, DateOnly from, DateOnly to, CancellationToken ct = default) { ... }
    public Task<IReadOnlyList<HealthSnapshot>> GetLatestSnapshotsAsync(CancellationToken ct = default) { ... }
    public Task<IReadOnlyList<string>> GetServiceNamesAsync(CancellationToken ct = default) { ... }
    public Task PurgeOlderThanAsync(DateOnly cutoff, CancellationToken ct = default) { ... }
}
```

```csharp
builder.Services
    .AddStatusPage(o => o.HistoryRetentionDays = 90)
    .UseStatusRepository<RedisStatusRepository>(ServiceLifetime.Singleton);
```

This replaces the default in-memory repository (or any previously registered provider) with your implementation. You can combine this with any additional service registrations your repository needs:

```csharp
var statusPage = builder.Services.AddStatusPage();

// Register your dependencies
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect("localhost"));

// Swap in your repository
statusPage.UseStatusRepository<RedisStatusRepository>(ServiceLifetime.Singleton);
```

## Choosing a Provider

| Scenario                       | Recommended         |
| ------------------------------ | ------------------- |
| Production, multi-instance     | PostgreSQL          |
| Existing MySQL infrastructure  | MySQL               |
| Single-instance, minimal setup | SQLite              |
| Development / testing          | In-memory (default) |

## Target Frameworks

All packages target `net8.0` and `net9.0`. The PostgreSQL and SQLite packages also support `net10.0`.

## License

See [LICENSE](LICENSE) for details.
