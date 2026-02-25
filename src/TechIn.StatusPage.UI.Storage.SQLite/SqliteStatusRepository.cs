using Microsoft.EntityFrameworkCore;
using TechIn.StatusPage.Core.Interfaces;
using TechIn.StatusPage.Core.Models;
using TechIn.StatusPage.Core.Models.Enums;
using TechIn.StatusPage.UI.Data;

namespace TechIn.StatusPage.UI.Storage.SQLite;

internal sealed class SqliteStatusRepository : IStatusRepository
{
    private readonly IDbContextFactory<StatusPageDbContext> _contextFactory;

    public SqliteStatusRepository(IDbContextFactory<StatusPageDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task SaveSnapshotsAsync(IEnumerable<HealthSnapshot> snapshots, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        context.Snapshots.AddRange(snapshots);
        await context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<DayAggregate>> GetDailyAggregatesAsync(
        string serviceName, DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var fromText = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc).ToString("O");
        var toText = to.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc).ToString("O");

        var snapshots = await context.Snapshots
            .AsNoTracking()
            .Where(s => s.ServiceName == serviceName)
            .ToListAsync(ct);

        var filtered = snapshots
            .Where(s => s.Timestamp.UtcDateTime >= from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)
                     && s.Timestamp.UtcDateTime <= to.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc))
            .ToList();

        var aggregates = filtered
            .GroupBy(s => DateOnly.FromDateTime(s.Timestamp.UtcDateTime))
            .Select(g => new DayAggregate
            {
                Date = g.Key,
                TotalChecks = g.Count(),
                HealthyChecks = g.Count(x => x.Status == ServiceStatus.Operational),
                DegradedChecks = g.Count(x => x.Status == ServiceStatus.Degraded),
                DownChecks = g.Count(x => x.Status == ServiceStatus.Down),
                Incidents = g
                    .Where(x => x.Status != ServiceStatus.Operational)
                    .OrderBy(x => x.Timestamp)
                    .Select(x => new IncidentEntry
                    {
                        Status = x.Status,
                        Timestamp = x.Timestamp,
                        Description = x.Description
                    })
                    .ToList()
            })
            .ToDictionary(a => a.Date);

        var result = new List<DayAggregate>();
        var current = from;

        while (current <= to)
        {
            result.Add(aggregates.TryGetValue(current, out var existing)
                ? existing
                : new DayAggregate
                {
                    Date = current,
                    TotalChecks = 0,
                    HealthyChecks = 0,
                    DegradedChecks = 0,
                    DownChecks = 0
                });
            current = current.AddDays(1);
        }

        return result;
    }

    public async Task<IReadOnlyList<HealthSnapshot>> GetLatestSnapshotsAsync(CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var all = await context.Snapshots
            .AsNoTracking()
            .ToListAsync(ct);

        return all
            .GroupBy(s => s.ServiceName)
            .Select(g => g.OrderByDescending(s => s.Timestamp).First())
            .ToList();
    }

    public async Task<IReadOnlyList<string>> GetServiceNamesAsync(CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        return await context.Snapshots
            .AsNoTracking()
            .Select(s => s.ServiceName)
            .Distinct()
            .OrderBy(n => n)
            .ToListAsync(ct);
    }

    public async Task PurgeOlderThanAsync(DateOnly cutoff, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var cutoffText = cutoff.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc).ToString("O");

        await context.Database.ExecuteSqlRawAsync(
            "DELETE FROM snapshots WHERE timestamp < {0}", [cutoffText], ct);
    }
}