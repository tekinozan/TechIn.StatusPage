namespace TechIn.StatusPage.Core.Models.Enums;


public enum ServiceStatus
{
    /// <summary>Green — fully operational.</summary>
    Operational = 0,

    /// <summary>Orange — degraded performance or partial outage.</summary>
    Degraded = 1,

    /// <summary>Red — full service outage.</summary>
    Down = 2
}
