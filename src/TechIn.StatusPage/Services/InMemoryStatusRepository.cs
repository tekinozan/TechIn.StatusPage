using System.Collections.Concurrent;
using TechIn.StatusPage.Core.Interfaces;
using TechIn.StatusPage.Core.Models;
using TechIn.StatusPage.Core.Models.Enums;

namespace TechIn.StatusPage.Hosting.Services;

/// <summary>
/// Thread-safe in-memory implementation of <see cref="IStatusRepository"/>.
/// Suitable for development and single-instance deployments.
/// For production multi-instance scenarios, swap in a Redis or SQL implementation.
/// </summary>
public sealed class InMemoryStatusRepository : IStatusRepository
{
    // Key: ServiceName, Value: list of snapshots (append-only, purged periodically)
    private readonly ConcurrentDictionary<string, List<HealthSnapshot>> _store = new();
    private readonly object _lock = new();

    public Task SaveSnapshotsAsync(IEnumerable<HealthSnapshot> snapshots, CancellationToken ct = default)
    {
        foreach (var snapshot in snapshots)
        {
            var list = _store.GetOrAdd(snapshot.ServiceName, _ => new List<HealthSnapshot>());
            lock (_lock)
            {
                list.Add(snapshot);
            }
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<DayAggregate>> GetDailyAggregatesAsync(
        string serviceName, DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        if (!_store.TryGetValue(serviceName, out var list))
            return Task.FromResult<IReadOnlyList<DayAggregate>>(Array.Empty<DayAggregate>());

        List<HealthSnapshot> snapshot;
        lock (_lock)
        {
            snapshot = list.ToList();
        }

        var aggregates = snapshot
            .Where(s => DateOnly.FromDateTime(s.Timestamp.UtcDateTime) >= from
                     && DateOnly.FromDateTime(s.Timestamp.UtcDateTime) <= to)
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
            .OrderBy(a => a.Date)
            .ToList();

        // Fill in missing days with "no data" (100% uptime assumed)
        var result = new List<DayAggregate>();
        var current = from;
        var lookup = aggregates.ToDictionary(a => a.Date);

        while (current <= to)
        {
            result.Add(lookup.TryGetValue(current, out var existing)
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

        return Task.FromResult<IReadOnlyList<DayAggregate>>(result);
    }

    public Task<IReadOnlyList<HealthSnapshot>> GetLatestSnapshotsAsync(CancellationToken ct = default)
    {
        var latest = new List<HealthSnapshot>();

        foreach (var (_, list) in _store)
        {
            lock (_lock)
            {
                var last = list.MaxBy(s => s.Timestamp);
                if (last is not null) latest.Add(last);
            }
        }

        return Task.FromResult<IReadOnlyList<HealthSnapshot>>(latest);
    }

    public Task<IReadOnlyList<string>> GetServiceNamesAsync(CancellationToken ct = default)
    {
        return Task.FromResult<IReadOnlyList<string>>(_store.Keys.OrderBy(k => k).ToList());
    }

    public Task PurgeOlderThanAsync(DateOnly cutoff, CancellationToken ct = default)
    {
        lock (_lock)
        {
            foreach (var (_, list) in _store)
            {
                list.RemoveAll(s => DateOnly.FromDateTime(s.Timestamp.UtcDateTime) < cutoff);
            }
        }

        return Task.CompletedTask;
    }
}