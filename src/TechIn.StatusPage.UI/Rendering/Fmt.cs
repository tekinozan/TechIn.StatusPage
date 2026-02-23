using System.Globalization;

namespace TechIn.StatusPage.UI.Rendering;

/// <summary>
/// Formatting utilities — escaping, dates, latency.
/// </summary>
internal static class Fmt
{
    public static string Esc(string value) =>
        System.Net.WebUtility.HtmlEncode(value);

    public static string Date(DateOnly date) =>
        date.ToString("MMM d", CultureInfo.InvariantCulture);

    public static string Timestamp(DateTimeOffset ts) =>
        ts.ToString("MMM dd, yyyy h:mm tt", CultureInfo.InvariantCulture) + " UTC";

    public static string Time(DateTimeOffset ts) =>
        ts.ToString("HH:mm", CultureInfo.InvariantCulture) + " UTC";

    public static string Latency(TimeSpan? latency) =>
        latency is null ? "" : $"{(int)latency.Value.TotalMilliseconds}ms";
}