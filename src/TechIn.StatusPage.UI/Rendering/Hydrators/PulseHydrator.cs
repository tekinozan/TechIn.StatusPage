using TechIn.StatusPage.Core.Models;

namespace TechIn.StatusPage.UI.Rendering.Hydrators;

internal sealed class PulseHydrator : TemplateHydratorBase
{
    protected override void AddCustomPlaceholders(Dictionary<string, string> placeholders, StatusPageResponse data, StatusPageOptions options)
    {
        placeholders["LOGO"] = BuildSharedLogo(options.LogoUrl);
    }

    protected override string BuildBanner(StatusPageResponse data, StatusPageOptions options)
    {
        var css = StatusMappings.Css(data.GlobalStatus);
        var uptimeStr = data.OverallUptime.ToString("F1") + "%";
        var noiseStr = (100.0 - data.OverallUptime).ToString("F1") + "%";
        var statusLabel = Fmt.Esc(data.GlobalStatusText).ToUpperInvariant();

        return $"""
        <div class="tracker-box">
            <div class="tracker-header">
                <span>System / <span style="color: var(--text)">Uptime strength</span></span>
                <span class="status-label {css}">{statusLabel}</span>
            </div>
            
            <div class="tracker-metrics">
                <div class="metric">
                    <div class="metric-label">SIGNAL STRENGTH</div>
                    <div class="metric-value">{uptimeStr} <span class="trend">+OP</span></div>
                </div>
                <div class="metric">
                    <div class="metric-label">NOISE RATIO</div>
                    <div class="metric-value" style="color: var(--text-muted)">{noiseStr}</div>
                </div>
            </div>

            <div class="progress-track">
                <div class="progress-fill {css}" style="width: {data.OverallUptime}%"></div>
            </div>
            
            <div class="progress-markers">
                <span>0%</span>
                <span>25%</span>
                <span>50%</span>
                <span>75%</span>
                <span>100%</span>
            </div>
        </div>
        """;
    }

    protected override string BuildServices(StatusPageResponse data, StatusPageOptions options)
    {
        if (data.Services.Count == 0)
            return """<div class="tracker-box" style="padding: 24px; text-align: center;">NO CHANNELS DETECTED</div>""";

        var rows = string.Concat(data.Services.Select((svc, idx) =>
        {
            var css = StatusMappings.Css(svc.CurrentStatus);
            var label = StatusMappings.Label(svc.CurrentStatus).ToUpperInvariant();

            var latency = options.ShowLatency && svc.LastLatency.HasValue
                ? $"""<span class="latency">{Fmt.Latency(svc.LastLatency)}</span>"""
                : "";

            return $"""
            <div class="channel-row">
                <div class="channel-info">
                    <span class="channel-name">{Fmt.Esc(svc.Name).ToUpperInvariant()}</span>
                    {latency}
                </div>
                <div class="channel-status {css}">{label}</div>
            </div>
            <div class="channel-uptime">
                {UptimeBarBuilder.Build(svc, idx)}
            </div>
            """;
        }));

        return $"""
        <div class="tracker-box">
            <div class="tracker-header" style="border-bottom: 1px solid var(--border); padding-bottom: 12px; margin-bottom: 16px;">
                <span>ACTIVE CHANNELS</span>
                <span style="color: var(--accent)">{data.Services.Count} INPUTS</span>
            </div>
            {rows}
        </div>
        """;
    }

    // Pulse has a custom, bracketed auto-refresh indicator, so we override the base method
    protected override string BuildAutoRefresh() =>
        """
        <div class="refresh-module" id="refresh-indicator">
            [ <span id="refresh-text">SYNCED</span> ]
        </div>
        """;
}