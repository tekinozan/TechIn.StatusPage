using TechIn.StatusPage.Core.Models.Enums;

namespace TechIn.StatusPage.Core.Models;

/// <summary>
/// Configuration options for the status page.
/// </summary>
public sealed class StatusPageOptions
{
    /// <summary>Page title displayed in the header.</summary>
    public string Title { get; set; } = "System Status";

    /// <summary>Number of days of history to display in the timeline bars.</summary>
    public int HistoryRetentionDays { get; set; } = 90;

    /// <summary>Interval in seconds between health check polls.</summary>
    public int PollingIntervalSeconds { get; set; } = 60;

    /// <summary>Optional company logo URL to display in the header.</summary>
    public string? LogoUrl { get; set; }

    /// <summary>Whether to show latency information per service.</summary>
    public bool ShowLatency { get; set; } = true;

    /// <summary>Whether to refresh the page auutomatically.</summary>
    public bool ActivateAutoRefresh { get; set; } = false;

    /// <summary>
    /// Optional predicate to filter which health checks appear on the status page.
    /// Return true to include, false to exclude.
    /// </summary>
    public Func<string, bool>? HealthCheckFilter { get; set; }

    /// <summary>
    /// Visual template to use. Each template includes its own built-in light/dark toggle.
    /// </summary>
    public StatusPageTemplate Template { get; set; } = StatusPageTemplate.Classic;

    // --- Secure Footer Options ---

    /// <summary>Whether to display the footer at the bottom of the page.</summary>
    public bool ShowFooter { get; set; } = true;

    /// <summary>Safe text displayed before the link.</summary>
    public string? FooterText { get; set; }

    /// <summary>Safe text displayed inside the link.</summary>
    public string? FooterLinkText { get; set; }

    /// <summary>The URL for the footer link.</summary>
    public string? FooterLinkUrl { get; set; }
}