using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using TechIn.StatusPage.Core.Interfaces;
using TechIn.StatusPage.Core.Models;
using TechIn.StatusPage.Core.Extensions;

namespace TechIn.StatusPage.Services;

/// <summary>
/// Background service that periodically polls registered health checks,
/// measures latency, and persists snapshots to the configured <see cref="IStatusRepository"/>.
/// </summary>
public sealed class HealthHistoryCollector : BackgroundService
{
    private readonly HealthCheckService _healthCheckService;
    private readonly IStatusRepository _repository;
    private readonly IOptionsMonitor<StatusPageOptions> _options;
    private readonly ILogger<HealthHistoryCollector> _logger;

    public HealthHistoryCollector(
        HealthCheckService healthCheckService,
        IStatusRepository repository,
        IOptionsMonitor<StatusPageOptions> options,
        ILogger<HealthHistoryCollector> logger)
    {
        _healthCheckService = healthCheckService;
        _repository = repository;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("StatusPage health collector starting. Polling every {Interval}s",
            _options.CurrentValue.PollingIntervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CollectAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during health check collection cycle");
            }

            await Task.Delay(
                TimeSpan.FromSeconds(_options.CurrentValue.PollingIntervalSeconds),
                stoppingToken);
        }
    }

    private async Task CollectAsync(CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        var report = await _healthCheckService.CheckHealthAsync(ct);
        sw.Stop();

        var filter = _options.CurrentValue.HealthCheckFilter;
        var now = DateTimeOffset.UtcNow;

        var snapshots = report.Entries
            .Where(e => filter is null || filter(e.Key))
            .Select(entry => new HealthSnapshot
            {
                ServiceName = entry.Key,
                Status = entry.Value.Status.ToServiceStatus(),
                Timestamp = now,
                Latency = entry.Value.Duration,
                Description = entry.Value.Description ?? entry.Value.Exception?.Message
            })
            .ToList();

        if (snapshots.Count > 0)
        {
            await _repository.SaveSnapshotsAsync(snapshots, ct);
            _logger.LogDebug("Collected {Count} health snapshots in {Elapsed}ms",
                snapshots.Count, sw.ElapsedMilliseconds);
        }

        // Periodic purge (runs every cycle but is cheap)
        var cutoff = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-_options.CurrentValue.HistoryRetentionDays));
        await _repository.PurgeOlderThanAsync(cutoff, ct);
    }
}