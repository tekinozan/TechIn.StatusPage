using TechIn.StatusPage.Core.Models.Enums;

namespace TechIn.StatusPage.Core.Models;

/// <summary>
/// A single incident event — represents a non-healthy check result for display purposes.
/// </summary>
public sealed record IncidentEntry
{
    public required ServiceStatus Status { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public string? Description { get; init; }
}