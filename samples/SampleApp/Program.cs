using Microsoft.Extensions.Diagnostics.HealthChecks;
using StatusPage.AspNetCore.Extensions;
using TechIn.StatusPage.Core.Models.Enums;
using TechIn.StatusPage.Hosting.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


// ── 1. Add the Status Page ────────────────────────────────────
builder.Services.AddStatusPage(options =>
{
    options.Title = "Acme Corp";
    options.HistoryRetentionDays = 90;
    options.PollingIntervalSeconds = 30;
    options.ShowLatency = true;
    options.ActivateAutoRefresh = false;
    options.ShowFooter = true;
    options.FooterText = "Powered By";
    options.FooterLinkText = "TechIn";
    options.FooterLinkUrl = "https://github.com/tekinozan";

    // Pick your template:
    options.Template = StatusPageTemplate.Axiom;   // dark-first, developer-oriented
    //options.Template = StatusPageTemplate.Pulse;   // cyber aesthetic
    // options.Template = StatusPageTemplate.Axiom; // clean, minimal

});

builder.Services.AddHealthChecks()
    .AddRedis("localhost:6379", "Redis")
    .AddCheck("API Server", () => HealthCheckResult.Healthy("Responding normally"))
    .AddCheck("Database", () =>
    {
        // Simulate occasional degradation
        return Random.Shared.Next(100) < 95
            ? HealthCheckResult.Healthy("Connected (12ms)")
            : HealthCheckResult.Degraded("Slow queries detected");
    })
    .AddCheck("Redis Cache", () => HealthCheckResult.Healthy("6 nodes online"))
    .AddCheck("Payment Gateway", () =>
    {
        // Simulate rare downtime
        return Random.Shared.Next(100) < 98
            ? HealthCheckResult.Healthy("Stripe API OK")
            : HealthCheckResult.Unhealthy("Connection timeout");
    })
    .AddCheck("Email Service", () => HealthCheckResult.Healthy("SendGrid OK"))
    .AddCheck("Search Index", () =>
    {
        return Random.Shared.Next(100) < 90
            ? HealthCheckResult.Healthy("Elasticsearch green")
            : HealthCheckResult.Degraded("Yellow cluster state");
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

// ── 3. Map the status page UI ────────────────────────────────
app.MapStatusPage("/status");

// Standard health endpoint still works
app.MapHealthChecks("/health");

app.MapGet("/", () => Results.Redirect("/status"));

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
