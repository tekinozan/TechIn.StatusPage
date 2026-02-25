# TechIn.StatusPage.UI.Storage.MySQL

MySQL storage provider for [TechIn.StatusPage](https://github.com/techin/statuspage). Persists health-check snapshots to a MySQL database using Entity Framework Core and the official Oracle MySQL connector.

## Installation

```bash
dotnet add package TechIn.StatusPage.UI.Storage.MySQL
```

## Usage

```csharp
builder.Services
    .AddStatusPage(o => o.HistoryRetentionDays = 90)
    .AddMySqlStorage(builder.Configuration.GetConnectionString("StatusPage")!);
```

That's it. The provider will automatically apply EF Core migrations on startup and begin persisting snapshots.

## Connection String

```json
{
  "ConnectionStrings": {
    "StatusPage": "Server=localhost;Port=3306;Database=statuspage;User=app;Password=secret;"
  }
}
```

## What It Registers

| Service                                  | Lifetime  | Description                                                          |
| ---------------------------------------- | --------- | -------------------------------------------------------------------- |
| `IDbContextFactory<StatusPageDbContext>` | Singleton | Configured with MySql.EntityFrameworkCore                            |
| `IStatusRepository`                      | Scoped    | EF Core implementation with `ExecuteDeleteAsync` for efficient purge |
| `DatabaseMigrationHostedService`         | Hosted    | Applies pending migrations at startup                                |

## Database Schema

MySQL does not support schemas, so tables are created directly in the target database:

```
snapshots
├── id              bigint (PK, auto-increment)
├── service_name    varchar(256)
├── status          varchar(32)     — Operational, Degraded, Down
├── timestamp       datetime(6)
├── latency         time(6)         — nullable
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
  --project src/TechIn.StatusPage.UI.Storage.MySQL \
  --framework net8.0
```

## Docker (for local development)

```bash
docker run -d \
  --name statuspage-mysql \
  -e MYSQL_ROOT_PASSWORD=root \
  -e MYSQL_DATABASE=statuspage \
  -p 3306:3306 \
  mysql:8
```

## Requirements

- MySQL 8.0 or later
- .NET 8.0 or 9.0

## Custom Repository

If you need a different storage backend, implement `IStatusRepository` and register it:

```csharp
builder.Services
    .AddStatusPage()
    .UseStatusRepository<YourCustomRepository>(ServiceLifetime.Scoped);
```

## License

MIT — see [LICENSE](LICENSE) for details.
