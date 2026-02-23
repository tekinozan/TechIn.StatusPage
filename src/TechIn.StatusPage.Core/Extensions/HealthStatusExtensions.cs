using Microsoft.Extensions.Diagnostics.HealthChecks;
using TechIn.StatusPage.Core.Models.Enums;

namespace TechIn.StatusPage.Core.Extensions;

public static class HealthStatusExtensions
{
    public static ServiceStatus ToServiceStatus(this HealthStatus status) => status switch
    {
        HealthStatus.Healthy => ServiceStatus.Operational,
        HealthStatus.Degraded => ServiceStatus.Degraded,
        HealthStatus.Unhealthy => ServiceStatus.Down,
        _ => ServiceStatus.Down
    };
}