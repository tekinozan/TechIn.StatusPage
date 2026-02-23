using System.Globalization;
using System.Text.Json;
using TechIn.StatusPage.Core.Models;
using TechIn.StatusPage.Core.Models.Enums;

namespace TechIn.StatusPage.UI.Rendering;

/// <summary>
/// Shared HTML snippet builders used across all templates.
/// </summary>
internal static class HtmlHelpers
{
    public static string Esc(string value) => System.Net.WebUtility.HtmlEncode(value);

    public static string StatusCss(ServiceStatus s) => s switch
    {
        ServiceStatus.Operational => "operational",
        ServiceStatus.Degraded => "degraded",
        ServiceStatus.Down => "down",
        _ => "operational"
    };

    public static string StatusLabel(ServiceStatus s) => s switch
    {
        ServiceStatus.Operational => "Operational",
        ServiceStatus.Degraded => "Degraded",
        ServiceStatus.Down => "Down",
        _ => "Unknown"
    };

    public static string StatusIcon(ServiceStatus s) => s switch
    {
        ServiceStatus.Operational => "✓",
        ServiceStatus.Degraded => "!",
        ServiceStatus.Down => "✕",
        _ => "?"
    };

    public static string FormatDate(DateOnly date) =>
        date.ToString("MMM d", CultureInfo.InvariantCulture);

    public static string FormatTimestamp(DateTimeOffset ts) =>
        ts.ToString("MMM dd, yyyy h:mm tt", CultureInfo.InvariantCulture) + " UTC";

    public static string FormatTime(DateTimeOffset ts) =>
        ts.ToString("HH:mm", CultureInfo.InvariantCulture) + " UTC";

    public static string FormatLatency(TimeSpan? latency)
    {
        if (latency is null) return "";
        return $"{(int)latency.Value.TotalMilliseconds}ms";
    }

    /// <summary>
    /// Renders the uptime bar strip for a service.
    /// Bars with incidents are clickable — clicking toggles a detail panel below.
    /// </summary>
    public static string RenderUptimeBars(ServiceSummary svc, int serviceIndex)
    {
        var history = svc.DailyHistory;
        if (history.Count == 0) return "";

        var bars = string.Concat(history.Select((day, dayIndex) =>
        {
            var hasIncidents = day.Incidents.Count > 0;
            var css = day.TotalChecks == 0 ? "nodata" : StatusCss(day.WorstStatus);
            var tip = day.TotalChecks == 0
                ? $"{FormatDate(day.Date)} · No data"
                : $"{FormatDate(day.Date)} · {day.UptimePercent:F1}%";

            var clickable = hasIncidents ? "has-incidents" : "";
            var clickAttr = hasIncidents
                ? $"""onclick="toggleIncidents(this,{serviceIndex},{dayIndex})" """
                : "";

            return $"""<div class="uptime-bar {css} {clickable}" data-tip="{Esc(tip)}" {clickAttr}></div>""";
        }));

        var first = FormatDate(history[0].Date);
        var last = FormatDate(history[^1].Date);

        // Pre-render incident data as a hidden JSON blob per service
        var incidentData = BuildIncidentDataJson(svc);

        return $"""
        <div class="uptime-bars">{bars}</div>
        <div class="uptime-footer">
            <span>{first}</span>
            <span>{svc.UptimePercentage:F2}% uptime</span>
            <span>{last}</span>
        </div>
        <div class="incident-panel" id="incidents-{serviceIndex}" style="display:none"></div>
        <script type="application/json" id="incident-data-{serviceIndex}">{incidentData}</script>
        """;
    }

    /// <summary>
    /// Serialises the incident entries for all days of a service into a JSON array-of-arrays.
    /// Index matches the day index in the uptime bars.
    /// </summary>
    private static string BuildIncidentDataJson(ServiceSummary svc)
    {
        var days = svc.DailyHistory.Select(day => day.Incidents.Select(i => new
        {
            status = StatusCss(i.Status),
            label = StatusLabel(i.Status),
            time = FormatTime(i.Timestamp),
            description = i.Description ?? ""
        }).ToArray()).ToArray();

        return JsonSerializer.Serialize(days, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    /// <summary>
    /// Returns the shared JavaScript for the incident panel toggle.
    /// Should be included once at the bottom of each template.
    /// </summary>
    public static string GetIncidentPanelScript() => """
    <script>
    function toggleIncidents(bar, serviceIdx, dayIdx) {
        var panel = document.getElementById('incidents-' + serviceIdx);
        var dataEl = document.getElementById('incident-data-' + serviceIdx);
        if (!panel || !dataEl) return;

        // If clicking the same bar again, close it
        if (panel.style.display !== 'none' && panel.dataset.activeDay === String(dayIdx)) {
            panel.style.display = 'none';
            panel.dataset.activeDay = '';
            bar.classList.remove('active-bar');
            return;
        }

        // Remove active state from previous bar
        var prev = panel.parentElement.querySelector('.active-bar');
        if (prev) prev.classList.remove('active-bar');
        bar.classList.add('active-bar');

        var allDays = JSON.parse(dataEl.textContent);
        var incidents = allDays[dayIdx] || [];

        if (incidents.length === 0) {
            panel.style.display = 'none';
            return;
        }

        var html = '<div class="incident-panel-header">' +
            '<span class="incident-panel-title">Events for this day</span>' +
            '<button class="incident-panel-close" onclick="this.closest(\'.incident-panel\').style.display=\'none\';var ab=this.closest(\'.service-row,.service-incidents-wrap\').querySelector(\'.active-bar\');if(ab)ab.classList.remove(\'active-bar\')">✕</button>' +
            '</div>';

        for (var i = 0; i < incidents.length; i++) {
            var inc = incidents[i];
            var desc = inc.description ? '<div class="incident-desc">' + escapeHtml(inc.description) + '</div>' : '';
            html += '<div class="incident-item">' +
                '<div class="incident-dot ' + inc.status + '"></div>' +
                '<div class="incident-content">' +
                    '<div class="incident-meta">' +
                        '<span class="incident-label ' + inc.status + '">' + inc.label + '</span>' +
                        '<span class="incident-time">' + inc.time + '</span>' +
                    '</div>' +
                    desc +
                '</div>' +
            '</div>';
        }

        panel.innerHTML = html;
        panel.style.display = 'block';
        panel.dataset.activeDay = String(dayIdx);
    }

    function escapeHtml(str) {
        var d = document.createElement('div');
        d.textContent = str;
        return d.innerHTML;
    }
    </script>
    """;

    /// <summary>
    /// Returns the shared CSS for the incident panel.
    /// Should be included in each template's style block or via {{INCIDENT_STYLES}}.
    /// </summary>
    public static string GetIncidentPanelCss() => """
    <style>
    .uptime-bar.has-incidents { cursor: pointer; }
    .uptime-bar.active-bar { outline: 2px solid var(--text); outline-offset: 1px; border-radius: 3px; z-index: 5; }
    .incident-panel {
        margin-top: 10px;
        background: var(--surface, #0c0c0e);
        border: 1px solid var(--border, rgba(255,255,255,0.06));
        border-radius: 8px;
        padding: 14px 18px;
        animation: fadeSlide 0.2s ease;
    }
    @keyframes fadeSlide {
        from { opacity: 0; transform: translateY(-4px); }
        to   { opacity: 1; transform: translateY(0); }
    }
    .incident-panel-header {
        display: flex;
        align-items: center;
        justify-content: space-between;
        margin-bottom: 12px;
        padding-bottom: 8px;
        border-bottom: 1px solid var(--border, rgba(255,255,255,0.06));
    }
    .incident-panel-title {
        font-size: 0.72rem;
        font-weight: 600;
        text-transform: uppercase;
        letter-spacing: 0.06em;
        color: var(--text-muted, rgba(232,232,236,0.5));
    }
    .incident-panel-close {
        background: none;
        border: none;
        color: var(--text-faint, rgba(232,232,236,0.25));
        cursor: pointer;
        font-size: 0.8rem;
        padding: 2px 6px;
        border-radius: 4px;
        transition: color 0.15s, background 0.15s;
    }
    .incident-panel-close:hover {
        color: var(--text, #e8e8ec);
        background: var(--border, rgba(255,255,255,0.06));
    }
    .incident-item {
        display: flex;
        align-items: flex-start;
        gap: 10px;
        padding: 8px 0;
        border-bottom: 1px solid color-mix(in srgb, var(--border, rgba(255,255,255,0.06)) 40%, transparent);
    }
    .incident-item:last-child { border-bottom: none; }
    .incident-dot {
        width: 8px;
        height: 8px;
        border-radius: 50%;
        margin-top: 4px;
        flex-shrink: 0;
    }
    .incident-dot.degraded { background: var(--degraded, #f59e0b); }
    .incident-dot.down     { background: var(--down, #ef4444); }
    .incident-content { flex: 1; min-width: 0; }
    .incident-meta {
        display: flex;
        align-items: center;
        gap: 10px;
        margin-bottom: 2px;
    }
    .incident-label {
        font-size: 0.72rem;
        font-weight: 700;
        text-transform: uppercase;
        letter-spacing: 0.03em;
    }
    .incident-label.degraded { color: var(--degraded, #f59e0b); }
    .incident-label.down     { color: var(--down, #ef4444); }
    .incident-time {
        font-family: 'JetBrains Mono', 'DM Mono', monospace;
        font-size: 0.62rem;
        color: var(--text-faint, rgba(232,232,236,0.25));
    }
    .incident-desc {
        font-size: 0.78rem;
        color: var(--text-muted, rgba(232,232,236,0.5));
        line-height: 1.4;
        margin-top: 2px;
    }
    </style>
    """;
}