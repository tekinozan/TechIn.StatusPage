using TechIn.StatusPage.Core.Models;

namespace TechIn.StatusPage.Core.Interfaces;

/// <summary>
/// Orchestrates data retrieval and builds the final <see cref="StatusPageResponse"/>.
/// </summary>
public interface IStatusPageService
{
    Task<StatusPageResponse> GetStatusAsync(CancellationToken ct = default);
}