using TechIn.StatusPage.Core.Models;

namespace TechIn.StatusPage.UI.Rendering.Hydrators;

internal abstract class TemplateHydratorBase
{
    private const string Incidents = "incidents";
    private const string Autorefresh = "autorefresh";

    /// <summary>
    /// Builds the core dictionary of placeholders used by all templates.
    /// </summary>
    public Dictionary<string, string> Build(StatusPageResponse data, StatusPageOptions options)
    {
        var placeholders = new Dictionary<string, string>
        {
            ["TITLE"] = Fmt.Esc(options.Title),
            ["BANNER"] = BuildBanner(data, options),
            ["SERVICES"] = BuildServices(data, options),
            ["INCIDENT_STYLES"] = TemplateEngine.LoadSharedCss(Incidents),
            ["INCIDENT_SCRIPT"] = TemplateEngine.LoadSharedJs(Incidents),
            ["ACTIVATE_AUTO_REFRESH"] = options.ActivateAutoRefresh ? BuildAutoRefresh() : string.Empty,
            ["AUTOREFRESH_STYLES"] = TemplateEngine.LoadSharedCss(Autorefresh),
            ["AUTOREFRESH_SCRIPT"] = TemplateEngine.LoadSharedJs(Autorefresh),
            ["REFRESH_INTERVAL"] = options.PollingIntervalSeconds.ToString(),
            ["FOOTER"] = BuildFooter(options)
        };

        // Hook for derived classes to add their own specific placeholders (e.g., LOGO, HEALTH_SCORE)
        AddCustomPlaceholders(placeholders, data, options);

        return placeholders;
    }

    // --- Abstract Methods (Must be implemented by derived classes) ---
    protected abstract string BuildBanner(StatusPageResponse data, StatusPageOptions options);
    protected abstract string BuildServices(StatusPageResponse data, StatusPageOptions options);

    // --- Virtual Methods (Can be overridden by derived classes if needed) ---
    protected virtual void AddCustomPlaceholders(Dictionary<string, string> placeholders, StatusPageResponse data, StatusPageOptions options)
    {
        // Default is a no-op.
    }

    protected virtual string BuildAutoRefresh()
    {
        // Default implementation shared by Classic and Axiom
        return """
        <div class="refresh-indicator" id="refresh-indicator">
            <svg class="refresh-ring" viewBox="0 0 16 16">
                <circle class="refresh-ring-bg" cx="8" cy="8" r="6" />
                <circle class="refresh-ring-fg" id="refresh-ring-fg" cx="8" cy="8" r="6" />
            </svg>
            <span id="refresh-text">Updated just now</span>
        </div>
        """;
    }

    protected virtual string BuildFooter(StatusPageOptions options)
    {
        if (!options.ShowFooter)
            return string.Empty;

        var text = string.IsNullOrWhiteSpace(options.FooterText) ? string.Empty : Fmt.Esc(options.FooterText).ToUpperInvariant();
        var linkText = string.IsNullOrWhiteSpace(options.FooterLinkText) ? string.Empty : Fmt.Esc(options.FooterLinkText).ToUpperInvariant();
        var linkUrl = string.IsNullOrWhiteSpace(options.FooterLinkUrl) ? string.Empty : Fmt.Esc(options.FooterLinkUrl);

        return $"""
        <footer>
             {text} <a href="{linkUrl}">{linkText}</a>
        </footer>
        """;
    }

    // --- Shared Helper Methods ---
    protected string BuildSharedLogo(string? url) =>
        string.IsNullOrWhiteSpace(url)
            ? string.Empty
            : $"""<img src="{Fmt.Esc(url)}" alt="Logo" class="logo" />""";
}