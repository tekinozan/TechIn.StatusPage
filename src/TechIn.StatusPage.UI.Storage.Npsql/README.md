# TechIn.StatusPage.UI.Storage.PostgreSQL

PostgreSQL storage provider for [TechIn.StatusPage](https://github.com/techin/statuspage). Persists health-check snapshots to a PostgreSQL database using Entity Framework Core and Npgsql.

## Installation

```bash
dotnet add package TechIn.StatusPage.UI.Storage.PostgreSQL
```

## Usage

```csharp
builder.Services
    .AddStatusPage(o => o.HistoryRetentionDays = 90)
    .AddPostgreSqlStorage(builder.Configuration.GetConnectionString("StatusPage")!);
```

That's it. The provider will automatically apply EF Core migrations on startup and begin persisting snapshots.

## Connection String

```json
{
  "ConnectionStrings": {
    "StatusPage": "Host=localhost;Port=5432;Database=statuspage;Username=app;Password=secret;"
  }
}
```

## What It Registers

| Service                                  | Lifetime  | Description                                                           |
| ---------------------------------------- | --------- | --------------------------------------------------------------------- |
| `IDbContextFactory<StatusPageDbContext>` | Singleton | Configured with Npgsql                                                |
| `IStatusRepository`                      | Scoped    | EF Core implementation using `ExecuteDeleteAsync` for efficient purge |
| `DatabaseMigrationHostedService`         | Hosted    | Applies pending migrations at startup                                 |

## Database Schema

Tables are created under the `status_page` schema:

```
status_page.snapshots
├── id              bigint (PK, auto-increment)
├── service_name    varchar(256)
├── status          varchar(32)     — Operational, Degraded, Down
├── timestamp       timestamptz
├── latency         interval        — nullable
└── description     varchar(2048)   — nullable
```

### Indexes

- `ix_snapshots_service_name` — filter by service
- `ix_snapshots_service_name_timestamp` — date-range queries per service
- `ix_snapshots_timestamp` — purge operations

## Manual Migration

If you prefer not to run migrations automatically, you can apply them via CLI:

```bash
dotnet ef database update \
  --project src/TechIn.StatusPage.UI.Storage.PostgreSQL \
  --framework net8.0
```

## Requirements

- PostgreSQL 12 or later
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
