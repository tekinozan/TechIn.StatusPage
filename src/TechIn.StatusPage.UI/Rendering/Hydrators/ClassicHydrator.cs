using TechIn.StatusPage.Core.Models;

namespace TechIn.StatusPage.UI.Rendering.Hydrators;

/// <summary>
/// Fills placeholders for the Classic template.
/// </summary>
internal sealed class ClassicHydrator : TemplateHydratorBase
{
    protected override void AddCustomPlaceholders(Dictionary<string, string> placeholders, StatusPageResponse data, StatusPageOptions options)
    {
        placeholders["LOGO"] = BuildSharedLogo(options.LogoUrl);
    }

    protected override string BuildBanner(StatusPageResponse data, StatusPageOptions options)
    {
        var css = StatusMappings.Css(data.GlobalStatus);
        var icon = StatusMappings.Icon(data.GlobalStatus);

        return $"""
        <div class="overall-banner {css}">
            <div class="overall-icon">{icon}</div>
            <div>
                <div>{Fmt.Esc(data.GlobalStatusText)}</div>
                <div style="font-size:0.78rem;opacity:0.7;margin-top:2px">
                    {data.OverallUptime:F2}% uptime over the monitored period
                </div>
            </div>
        </div>
        """;
    }

    protected override string BuildServices(StatusPageResponse data, StatusPageOptions options)
    {
        if (data.Services.Count == 0)
            return """<p style="padding:28px 0;font-size:0.85rem;color:var(--text-muted)">No services registered yet.</p>""";

        var rows = string.Concat(data.Services.Select((svc, idx) =>
        {
            var css = StatusMappings.Css(svc.CurrentStatus);
            var label = StatusMappings.Label(svc.CurrentStatus);

            var latency = options.ShowLatency && svc.LastLatency.HasValue
                ? $"""<span class="response-time">{Fmt.Latency(svc.LastLatency)}</span>"""
                : "";

            return $"""
            <div class="service-row">
                <div class="service-top">
                    <span class="service-name">{Fmt.Esc(svc.Name)}</span>
                    <div class="service-right">
                        {latency}
                        <span class="status-badge {css}">{label}</span>
                    </div>
                </div>
                {UptimeBarBuilder.Build(svc, idx)}
            </div>
            """;
        }));

        return $"""
        <div class="group-label">Services</div>
        {rows}
        """;
    }

    // Classic uses a specific layout for its footer, so we override the base implementation
    protected override string BuildFooter(StatusPageOptions options)
    {
        if (!options.ShowFooter)
            return string.Empty;

        var text = string.IsNullOrWhiteSpace(options.FooterText) ? string.Empty : Fmt.Esc(options.FooterText);
        var linkText = string.IsNullOrWhiteSpace(options.FooterLinkText) ? string.Empty : Fmt.Esc(options.FooterLinkText);
        var linkUrl = string.IsNullOrWhiteSpace(options.FooterLinkUrl) ? string.Empty : Fmt.Esc(options.FooterLinkUrl);

        return $"""
        <footer>
            <div class="container">{text} <a href="{linkUrl}">{linkText}</a></div>
        </footer>
        """;
    }
}