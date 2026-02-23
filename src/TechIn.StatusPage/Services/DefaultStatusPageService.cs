using Microsoft.Extensions.Options;
using TechIn.StatusPage.Core.Interfaces;
using TechIn.StatusPage.Core.Models;
using TechIn.StatusPage.Core.Models.Enums;

namespace TechIn.StatusPage.Hosting.Services;

/// <summary>
/// Default implementation of <see cref="IStatusPageService"/> that reads from
/// the repository and assembles the <see cref="StatusPageResponse"/>.
/// </summary>
public sealed class DefaultStatusPageService : IStatusPageService
{
    private readonly IStatusRepository _repository;
    private readonly IOptionsMonitor<StatusPageOptions> _options;

    public DefaultStatusPageService(IStatusRepository repository, IOptionsMonitor<StatusPageOptions> options)
    {
        _repository = repository;
        _options = options;
    }

    public async Task<StatusPageResponse> GetStatusAsync(CancellationToken ct = default)
    {
        var opts = _options.CurrentValue;
        var serviceNames = await _repository.GetServiceNamesAsync(ct);
        var latestSnapshots = await _repository.GetLatestSnapshotsAsync(ct);
        var latestLookup = latestSnapshots.ToDictionary(s => s.ServiceName);

        var to = DateOnly.FromDateTime(DateTime.UtcNow);
        var from = to.AddDays(-opts.HistoryRetentionDays);

        var services = new List<ServiceSummary>();

        foreach (var name in serviceNames)
        {
            var dailyHistory = await _repository.GetDailyAggregatesAsync(name, from, to, ct);

            var totalChecks = dailyHistory.Sum(d => d.TotalChecks);
            var healthyChecks = dailyHistory.Sum(d => d.HealthyChecks);
            var uptimePercent = totalChecks == 0
                ? 100.0
                : Math.Round((double)healthyChecks / totalChecks * 100, 2);

            latestLookup.TryGetValue(name, out var latest);

            services.Add(new ServiceSummary
            {
                Name = name,
                CurrentStatus = latest?.Status ?? ServiceStatus.Operational,
                UptimePercentage = uptimePercent,
                DailyHistory = dailyHistory,
                LastLatency = opts.ShowLatency ? latest?.Latency : null
            });
        }

        var globalStatus = services.Count == 0
            ? ServiceStatus.Operational
            : services.Max(s => s.CurrentStatus);

        var overallUptime = services.Count == 0
            ? 100.0
            : Math.Round(services.Average(s => s.UptimePercentage), 2);

        return new StatusPageResponse
        {
            Title = opts.Title,
            GlobalStatus = globalStatus,
            OverallUptime = overallUptime,
            LastUpdated = DateTimeOffset.UtcNow,
            Services = services
        };
    }
}