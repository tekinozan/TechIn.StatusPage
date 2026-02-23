using System.Text.Json.Serialization;
using TechIn.StatusPage.Core.Models.Enums;

namespace TechIn.StatusPage.Core.Models;

/// <summary>
/// The complete response payload served to the status page UI.
/// </summary>
public sealed record StatusPageResponse
{
    public required string Title { get; init; }
    public required ServiceStatus GlobalStatus { get; init; }

    [JsonPropertyName("globalStatusText")]
    public string GlobalStatusText => GlobalStatus switch
    {
        ServiceStatus.Operational => "All Systems Operational",
        ServiceStatus.Degraded => "Partial System Outage",
        ServiceStatus.Down => "Major System Outage",
        _ => "Unknown"
    };

    public required double OverallUptime { get; init; }
    public required DateTimeOffset LastUpdated { get; init; }
    public required IReadOnlyList<ServiceSummary> Services { get; init; }
}
