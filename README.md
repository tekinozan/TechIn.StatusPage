# TechIn.StatusPage

A beautiful, developer-first status page and health check dashboard for ASP.NET Core applications.

TechIn.StatusPage seamlessly transforms your native `.NET HealthChecks` into a sleek, public-facing portal. It tracks uptime history, visualizes system stability, monitors latency, and automatically reports outages with a modern, minimalist interface.

## ✨ Features

- **Native Integration:** Hooks directly into `Microsoft.Extensions.Diagnostics.HealthChecks`. No new health check logic required.
- **Uptime History:** Visualizes past system performance and partial outages over a configurable retention period.
- **Multiple Themes:** Choose from beautifully crafted, responsive templates like `Axiom` (dark-first, developer-oriented) or `Pulse` (cyber aesthetic).
- **Latency Tracking:** Displays real-time response times for your services.
- **Zero Static Files:** Rendered entirely dynamically. No need to pollute your `wwwroot` directory.
- **Highly Customizable:** Control footers, auto-refresh intervals, and retention days right from your `Program.cs`.

## 📦 Installation

Install the main package via NuGet. This umbrella package automatically includes the required Core storage engine and the UI rendering library.

```bash
dotnet add package TechIn.StatusPage
```

_(Note: Ensure you also have your standard `AspNetCore.HealthChecks._` packages installed for the specific services you want to monitor).\*

## 🚀 Quick Start & Configuration

TechIn.StatusPage is designed to be highly configurable while requiring minimal setup. Add the services and map the endpoint in your `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

// 1. Configure TechIn Status Page Options
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
    // options.Template = StatusPageTemplate.Pulse;   // cyber aesthetic
    // options.Template = StatusPageTemplate.Axiom; // clean, minimal
});

// 2. Register your standard .NET Health Checks
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

// 3. Map the TechIn Status Page UI endpoint
app.MapStatusPage("/status");

// Standard health endpoint still works perfectly
app.MapHealthChecks("/health");

// Optional: Redirect root to status page
app.MapGet("/", () => Results.Redirect("/status"));

app.Run();

```

## 🏗️ Architecture

This package is built with a clean, decoupled architecture:

- **TechIn.StatusPage.Core:** The background publisher and state management engine (tracks history in-memory).
- **TechIn.StatusPage.UI:** The dynamic Razor rendering layer.
- **TechIn.StatusPage (Hosting):** The wrapper that brings them together with easy-to-use extension methods for your `Program.cs`.

## 📄 License

This project is licensed under the MIT License.
