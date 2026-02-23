using System.Text;
using System.Text.Json;
using TechIn.StatusPage.Core.Models;

namespace TechIn.StatusPage.UI.Rendering;

/// <summary>
/// Builds the uptime bar strip and incident data for a single service.
/// Uses StringBuilder for efficient string building with many bars.
/// </summary>
internal static class UptimeBarBuilder
{
    // Reuse across calls — thread-safe because JsonSerializerOptions is immutable after first use
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Renders the complete uptime bar section: bars + footer + incident panel + data.
    /// </summary>
    public static string Build(ServiceSummary svc, int serviceIndex)
    {
        var history = svc.DailyHistory;
        if (history.Count == 0) return "";

        // Pre-size: ~120 chars per bar × 90 days ≈ 11KB
        var sb = new StringBuilder(history.Count * 128);

        // ── Bars ────────────────────────────────────────────
        sb.Append("""<div class="uptime-bars">""");

        for (var i = 0; i < history.Count; i++)
        {
            var day = history[i];
            var hasIncidents = day.Incidents.Count > 0;

            var css = day.TotalChecks == 0 ? "nodata" : StatusMappings.Css(day.WorstStatus);
            var tip = day.TotalChecks == 0
                ? $"{Fmt.Date(day.Date)} · No data"
                : $"{Fmt.Date(day.Date)} · {day.UptimePercent:F1}%";

            sb.Append($"""<div class="uptime-bar {css}""");

            if (hasIncidents)
            {
                sb.Append(" has-incidents");
            }

            sb.Append($"""" data-tip="{Fmt.Esc(tip)}"""");

            if (hasIncidents)
            {
                sb.Append($""" onclick="toggleIncidents(this,{serviceIndex},{i})" """);
            }

            sb.Append("></div>");
        }

        sb.AppendLine("</div>");

        // ── Footer ──────────────────────────────────────────
        var first = Fmt.Date(history[0].Date);
        var last = Fmt.Date(history[^1].Date);
        sb.AppendLine($"""
        <div class="uptime-footer">
            <span>{first}</span>
            <span>{svc.UptimePercentage:F2}% uptime</span>
            <span>{last}</span>
        </div>
        """);

        // ── Incident panel (initially hidden) ───────────────
        sb.AppendLine($"""<div class="incident-panel" id="incidents-{serviceIndex}" style="display:none"></div>""");

        // ── Incident data (JSON blob) ───────────────────────
        var json = SerializeIncidents(svc);
        sb.AppendLine($"""<script type="application/json" id="incident-data-{serviceIndex}">{json}</script>""");

        return sb.ToString();
    }

    /// <summary>
    /// Serializes incidents as an array-of-arrays (indexed by day position).
    /// Only serializes non-empty days to keep payload small.
    /// </summary>
    private static string SerializeIncidents(ServiceSummary svc)
    {
        var days = new object[svc.DailyHistory.Count];

        for (var i = 0; i < svc.DailyHistory.Count; i++)
        {
            var incidents = svc.DailyHistory[i].Incidents;
            if (incidents.Count == 0)
            {
                days[i] = Array.Empty<object>();
                continue;
            }

            days[i] = incidents.Select(inc => new
            {
                status = StatusMappings.Css(inc.Status),
                label = StatusMappings.Label(inc.Status),
                time = Fmt.Time(inc.Timestamp),
                description = inc.Description ?? ""
            }).ToArray();
        }

        return JsonSerializer.Serialize(days, JsonOpts);
    }
}