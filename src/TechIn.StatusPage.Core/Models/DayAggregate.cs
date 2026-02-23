using TechIn.StatusPage.Core.Models.Enums;

namespace TechIn.StatusPage.Core.Models;

/// <summary>
/// Aggregated health data for a single day, used to render a single "bar" in the timeline.
/// </summary>
public sealed record DayAggregate
{
    public required DateOnly Date { get; init; }

    /// <summary>Total checks performed on this day.</summary>
    public required int TotalChecks { get; init; }

    /// <summary>Number of checks that returned <see cref="ServiceStatus.Operational"/>.</summary>
    public required int HealthyChecks { get; init; }

    /// <summary>Number of checks that returned <see cref="ServiceStatus.Degraded"/>.</summary>
    public required int DegradedChecks { get; init; }

    /// <summary>Number of checks that returned <see cref="ServiceStatus.Down"/>.</summary>
    public required int DownChecks { get; init; }

    /// <summary>Uptime percentage for this day (0.0 – 100.0).</summary>
    public double UptimePercent => TotalChecks == 0
        ? 100.0
        : Math.Round((double)HealthyChecks / TotalChecks * 100, 2);

    /// <summary>The worst status observed during the day.</summary>
    public ServiceStatus WorstStatus
    {
        get
        {
            if (DownChecks > 0) return ServiceStatus.Down;
            if (DegradedChecks > 0) return ServiceStatus.Degraded;
            return ServiceStatus.Operational;
        }
    }

    /// <summary>
    /// Non-healthy events (Degraded / Down) that occurred on this day, ordered by timestamp.
    /// Empty if the day was fully operational or has no data.
    /// </summary>
    public IReadOnlyList<IncidentEntry> Incidents { get; init; } = [];
}