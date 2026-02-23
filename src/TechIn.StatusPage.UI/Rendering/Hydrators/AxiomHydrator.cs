using TechIn.StatusPage.Core.Models;

namespace TechIn.StatusPage.UI.Rendering.Hydrators;

/// <summary>
/// Fills placeholders for the Axiom template.
/// </summary>
internal sealed class AxiomHydrator : TemplateHydratorBase
{
    protected override void AddCustomPlaceholders(Dictionary<string, string> placeholders, StatusPageResponse data, StatusPageOptions options)
    {
        var healthScore = (int)Math.Round(data.OverallUptime);

        // Add Axiom-specific dashboard data placeholders
        placeholders["BRAND_ICON"] = BuildBrandIcon(options);
        placeholders["RETENTION_DAYS"] = options.HistoryRetentionDays.ToString();
        placeholders["HEALTH_SCORE"] = healthScore.ToString();
        placeholders["OVERALL_UPTIME"] = $"{data.OverallUptime:F2}";
        placeholders["SERVICE_COUNT"] = data.Services.Count.ToString();
    }

    private static string BuildBrandIcon(StatusPageOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.LogoUrl))
            return $"""<div class="brand-icon"><img src="{Fmt.Esc(options.LogoUrl)}" alt="Logo" /></div>""";

        var initials = options.Title.Length >= 2 ? options.Title[..2] : options.Title;
        return $"""<div class="brand-icon">{Fmt.Esc(initials)}</div>""";
    }

    protected override string BuildBanner(StatusPageResponse data, StatusPageOptions options)
    {
        var css = StatusMappings.Css(data.GlobalStatus);

        return $"""
        <div class="status-hero {css} fade-in stagger-1">
            <div class="status-hero-dot"></div>
            <div class="status-hero-text">{Fmt.Esc(data.GlobalStatusText)}</div>
            <div class="status-hero-time">{Fmt.Timestamp(data.LastUpdated)}</div>
        </div>
        """;
    }

    protected override string BuildServices(StatusPageResponse data, StatusPageOptions options)
    {
        if (data.Services.Count == 0)
            return """<div class="service-row"><div class="service-name" style="color:var(--text-muted)">No services registered yet.</div></div>""";

        var stagger = 3;
        return string.Concat(data.Services.Select((svc, idx) =>
        {
            var css = StatusMappings.Css(svc.CurrentStatus);
            var label = StatusMappings.Label(svc.CurrentStatus);
            var delay = $"stagger-{Math.Min(stagger++, 7)}";

            var latency = options.ShowLatency && svc.LastLatency.HasValue
                ? $"""<span class="response-time">{Fmt.Latency(svc.LastLatency)}</span>"""
                : "";

            return $"""
            <div class="service-row fade-in {delay}">
                <div class="service-top">
                    <div class="service-name">{Fmt.Esc(svc.Name)}</div>
                    <div class="service-right">
                        {latency}
                        <span class="badge {css}">{label}</span>
                    </div>
                </div>
                {UptimeBarBuilder.Build(svc, idx)}
            </div>
            """;
        }));
    }

    protected override string BuildFooter(StatusPageOptions options)
    {
        if (!options.ShowFooter)
            return string.Empty;

        var text = string.IsNullOrWhiteSpace(options.FooterText) ? string.Empty : Fmt.Esc(options.FooterText).ToUpperInvariant();
        var linkText = string.IsNullOrWhiteSpace(options.FooterLinkText) ? string.Empty : Fmt.Esc(options.FooterLinkText).ToUpperInvariant();
        var linkUrl = string.IsNullOrWhiteSpace(options.FooterLinkUrl) ? string.Empty : Fmt.Esc(options.FooterLinkUrl);

        return $"""
        <footer class="site-footer">
            <div class="container">
                {text} <a href="{linkUrl}">{linkText}</a> 
            </div>
        </footer>
        """;
    }
}