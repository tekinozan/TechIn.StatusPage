using TechIn.StatusPage.Core.Models.Enums;

namespace TechIn.StatusPage.Core.Models;

/// <summary>
/// Full summary for a single service, including current status and historical timeline.
/// </summary>
public sealed record ServiceSummary
{
    public required string Name { get; init; }
    public required ServiceStatus CurrentStatus { get; init; }
    public required double UptimePercentage { get; init; }
    public required IReadOnlyList<DayAggregate> DailyHistory { get; init; }
    public TimeSpan? LastLatency { get; init; }
}
