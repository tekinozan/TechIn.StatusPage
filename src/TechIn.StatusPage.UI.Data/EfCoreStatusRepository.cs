using Microsoft.EntityFrameworkCore;
using TechIn.StatusPage.Core.Interfaces;
using TechIn.StatusPage.Core.Models;
using TechIn.StatusPage.Core.Models.Enums;

namespace TechIn.StatusPage.UI.Data;

public sealed class EfCoreStatusRepository : IStatusRepository
{
    private readonly IDbContextFactory<StatusPageDbContext> _contextFactory;

    public EfCoreStatusRepository(IDbContextFactory<StatusPageDbContext> contextFactory)
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

        var fromDto = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var toDto = to.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        // Fetch filtered data from DB
        var snapshots = await context.Snapshots
            .AsNoTracking()
            .Where(s => s.ServiceName == serviceName
                     && s.Timestamp >= fromDto
                     && s.Timestamp <= toDto)
            .OrderBy(s => s.Timestamp)
            .ToListAsync(ct);

        // Aggregate in memory
        var aggregates = snapshots
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

        // Fill missing days
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

        // Uses a correlated subquery — efficient with the (ServiceName, Timestamp) index
        var latest = await context.Snapshots
            .AsNoTracking()
            .GroupBy(s => s.ServiceName)
            .Select(g => g.OrderByDescending(s => s.Timestamp).First())
            .ToListAsync(ct);

        return latest;
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

        var cutoffDto = cutoff.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        // EF Core 7+ ExecuteDeleteAsync — single SQL DELETE, no entity loading
        await context.Snapshots
            .Where(s => s.Timestamp < cutoffDto)
            .ExecuteDeleteAsync(ct);
    }
}
