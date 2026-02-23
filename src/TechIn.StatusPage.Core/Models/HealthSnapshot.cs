using TechIn.StatusPage.Core.Models.Enums;

namespace TechIn.StatusPage.Core.Models;

public sealed record HealthSnapshot
{
    public required string ServiceName { get; init; }
    public required ServiceStatus Status { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public TimeSpan? Latency { get; init; }
    public string? Description { get; init; }
}
