namespace TechIn.StatusPage.Core.Models.Enums;

/// <summary>
/// Available visual templates for the status page.
/// Each template ships with its own light/dark toggle built in.
/// </summary>
public enum StatusPageTemplate
{
    /// <summary>Clean, minimal — DM Sans typography, soft rounded cards.</summary>
    Classic = 0,

    /// <summary>Dark-first, developer-oriented — JetBrains Mono, latency chart, noise texture.</summary>
    Axiom = 1,
    Pulse = 2
}