using TechIn.StatusPage.Core.Models.Enums;

namespace TechIn.StatusPage.UI.Rendering;

/// <summary>
/// Pure status-to-string mappings. No HTML, no side effects.
/// </summary>
internal static class StatusMappings
{
    public static string Css(ServiceStatus s) => s switch
    {
        ServiceStatus.Operational => "operational",
        ServiceStatus.Degraded => "degraded",
        ServiceStatus.Down => "down",
        _ => "operational"
    };

    public static string Label(ServiceStatus s) => s switch
    {
        ServiceStatus.Operational => "Operational",
        ServiceStatus.Degraded => "Degraded",
        ServiceStatus.Down => "Down",
        _ => "Unknown"
    };

    public static string Icon(ServiceStatus s) => s switch
    {
        ServiceStatus.Operational => "✓",
        ServiceStatus.Degraded => "!",
        ServiceStatus.Down => "✕",
        _ => "?"
    };
}