# TechIn.StatusPage.UI.Storage.Sqlite

SQLite storage provider for [TechIn.StatusPage](https://github.com/techin/statuspage). Persists health-check snapshots to a local SQLite database using Entity Framework Core. Ideal for single-instance deployments, small projects, and local development.

## Installation

```bash
dotnet add package TechIn.StatusPage.UI.Storage.Sqlite
```

## Usage

```csharp
builder.Services
    .AddStatusPage(o => o.HistoryRetentionDays = 90)
    .AddSqliteStorage("Data Source=statuspage.db");
```

That's it. The provider will automatically apply EF Core migrations on startup and begin persisting snapshots. The database file is created automatically if it doesn't exist.

## Connection String

```json
{
  "ConnectionStrings": {
    "StatusPage": "Data Source=statuspage.db"
  }
}
```

Or use an absolute path:

```json
{
  "ConnectionStrings": {
    "StatusPage": "Data Source=/var/data/statuspage.db"
  }
}
```

The default connection string when none is provided is `Data Source=statuspage.db`.

## What It Registers

| Service                                  | Lifetime  | Description                                                                                 |
| ---------------------------------------- | --------- | ------------------------------------------------------------------------------------------- |
| `IDbContextFactory<StatusPageDbContext>` | Singleton | Configured with Microsoft.EntityFrameworkCore.Sqlite                                        |
| `IStatusRepository`                      | Scoped    | SQLite-optimized implementation with client-side evaluation for `DateTimeOffset` operations |
| `DatabaseMigrationHostedService`         | Hosted    | Applies pending migrations at startup                                                       |

## Database Schema

```
snapshots
├── id              integer (PK, autoincrement)
├── service_name    text
├── status          text            — Operational, Degraded, Down
├── timestamp       text            — ISO 8601 format
├── latency         text            — nullable
└── description     text            — nullable
```

### Indexes

- `ix_snapshots_service_name` — filter by service
- `ix_snapshots_service_name_timestamp` — date-range queries per service
- `ix_snapshots_timestamp` — purge operations

## Manual Migration

If you prefer not to run migrations automatically, you can apply them via CLI:

```bash
dotnet ef database update \
  --project src/TechIn.StatusPage.UI.Storage.Sqlite \
  --framework net8.0
```

## SQLite Considerations

- **Single-writer**: SQLite allows only one write at a time. This is fine for typical status page workloads (one write per poll cycle), but not suitable for high-concurrency scenarios.
- **DateTimeOffset**: SQLite stores `DateTimeOffset` as ISO 8601 text. Some LINQ operations (comparisons, ordering) are evaluated client-side to ensure correctness.
- **File-based**: The database is a single file on disk. Back it up by copying the file. No server process required.
- **No schema support**: Tables are created without a schema prefix.

## Requirements

- .NET 8.0, 9.0, or 10.0

## Custom Repository

If you need a different storage backend, implement `IStatusRepository` and register it:

```csharp
builder.Services
    .AddStatusPage()
    .UseStatusRepository<YourCustomRepository>(ServiceLifetime.Scoped);
```

## License

MIT — see [LICENSE](LICENSE) for details.
