using TechIn.StatusPage.Core.Models;

namespace TechIn.StatusPage.Core.Interfaces;

/// <summary>
/// Abstraction for storing and retrieving health check history.
/// Implement this interface for custom storage backends (Redis, SQL, etc.).
/// </summary>
public interface IStatusRepository
{
    /// <summary>
    /// Persists a batch of health snapshots (one per service per poll cycle).
    /// </summary>
    Task SaveSnapshotsAsync(IEnumerable<HealthSnapshot> snapshots, CancellationToken ct = default);

    /// <summary>
    /// Retrieves daily aggregates for a specific service within a date range.
    /// </summary>
    Task<IReadOnlyList<DayAggregate>> GetDailyAggregatesAsync(
        string serviceName,
        DateOnly from,
        DateOnly to,
        CancellationToken ct = default);

    /// <summary>
    /// Gets the most recent snapshot for each known service.
    /// </summary>
    Task<IReadOnlyList<HealthSnapshot>> GetLatestSnapshotsAsync(CancellationToken ct = default);

    /// <summary>
    /// Returns all distinct service names that have ever been recorded.
    /// </summary>
    Task<IReadOnlyList<string>> GetServiceNamesAsync(CancellationToken ct = default);

    /// <summary>
    /// Purges history older than the specified date.
    /// </summary>
    Task PurgeOlderThanAsync(DateOnly cutoff, CancellationToken ct = default);
}