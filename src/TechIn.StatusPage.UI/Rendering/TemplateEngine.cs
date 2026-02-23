using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace TechIn.StatusPage.UI.Rendering;

/// <summary>
/// Loads embedded HTML/CSS/JS resources and performs efficient placeholder replacement.
/// All resources are cached after first load (thread-safe, lock-free reads).
/// </summary>
internal static partial class TemplateEngine
{
    private static readonly Assembly Asm = typeof(TemplateEngine).Assembly;
    private static readonly ConcurrentDictionary<string, string> Cache = new();

    // Pre-compiled regex: matches {{KEY}} where KEY = uppercase + underscore
    [GeneratedRegex(@"\{\{([A-Z_]+)\}\}", RegexOptions.Compiled)]
    private static partial Regex PlaceholderPattern();

    /// <summary>
    /// Loads an embedded resource by logical name.
    /// "Classic" → TechIn.StatusPage.UI.Templates.Classic.html
    /// "Shared/incidents.css" → TechIn.StatusPage.UI.Templates.Shared.incidents.css
    /// </summary>
    public static string Load(string name)
    {
        return Cache.GetOrAdd(name, static key =>
        {
            // Add .html extension if the caller didn't specify one
            var fileName = Path.HasExtension(key) ? key : $"{key}.html";
            var resourcePath = $"TechIn.StatusPage.UI.Templates.{fileName.Replace('/', '.')}";

            using var stream = Asm.GetManifestResourceStream(resourcePath)
                ?? throw new InvalidOperationException(
                    $"Embedded resource not found: {resourcePath}. " +
                    $"Available: [{string.Join(", ", Asm.GetManifestResourceNames())}]");

            using var reader = new StreamReader(stream, Encoding.UTF8);
            return reader.ReadToEnd();
        });
    }

    /// <summary>
    /// Replaces all {{KEY}} placeholders using a single StringBuilder pass.
    /// </summary>
    public static string Hydrate(string template, Dictionary<string, string> values)
    {
        return PlaceholderPattern().Replace(template, match =>
        {
            var key = match.Groups[1].Value;
            return values.TryGetValue(key, out var replacement) ? replacement : match.Value;
        });
    }

    /// <summary>
    /// Loads shared CSS and wraps it in a style tag.
    /// </summary>
    public static string LoadSharedCss(string name)
    {
        var css = Load($"Shared/css/{name}.css");
        return $"<style>{css}</style>";
    }

    /// <summary>
    /// Loads shared JS and wraps it in a script tag.
    /// </summary>
    public static string LoadSharedJs(string name)
    {
        var js = Load($"Shared/js/{name}.js");
        return $"<script>{js}</script>";
    }
}