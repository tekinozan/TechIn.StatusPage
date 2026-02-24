using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;
using TechIn.StatusPage.Core.Interfaces;
using TechIn.StatusPage.Core.Models;
using TechIn.StatusPage.UI;

namespace TechIn.StatusPage.Extensions;

public static class EndpointRouteBuilderExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Maps the status page UI at the given path and a JSON API at <c>{path}/api</c>.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="path">URL path for the status page (default: <c>/status</c>).</param>
    /// <returns>A convention builder for further customization.</returns>
    public static IEndpointRouteBuilder MapStatusPage(
        this IEndpointRouteBuilder endpoints,
        string path = "/status")
    {
        var normalizedPath = path.TrimEnd('/');

        // JSON API endpoint
        endpoints.MapGet($"{normalizedPath}/api", async (HttpContext context, CancellationToken ct) =>
        {
            var service = context.RequestServices.GetRequiredService<IStatusPageService>();
            var response = await service.GetStatusAsync(ct);
            context.Response.ContentType = "application/json";
            context.Response.Headers.CacheControl = "no-cache, no-store";
            await JsonSerializer.SerializeAsync(context.Response.Body, response, JsonOptions, ct);
        })
        .ExcludeFromDescription(); // Hide from OpenAPI/Swagger

        // HTML UI endpoint
        endpoints.MapGet(normalizedPath, async (HttpContext context, CancellationToken ct) =>
        {
            var service = context.RequestServices.GetRequiredService<IStatusPageService>();
            var options = context.RequestServices.GetRequiredService<IOptionsMonitor<StatusPageOptions>>();
            var response = await service.GetStatusAsync(ct);

            var html = StatusPageRenderer.Render(response, options.CurrentValue);
            context.Response.ContentType = "text/html; charset=utf-8";
            context.Response.Headers.CacheControl = "no-cache, no-store";
            await context.Response.WriteAsync(html, ct);
        })
        .ExcludeFromDescription();

        return endpoints;
    }
}