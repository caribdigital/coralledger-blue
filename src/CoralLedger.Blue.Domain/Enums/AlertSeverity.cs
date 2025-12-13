namespace CoralLedger.Blue.Domain.Enums;

/// <summary>
/// Severity level of an alert
/// </summary>
public enum AlertSeverity
{
    /// <summary>
    /// Informational - no action required
    /// </summary>
    Info = 1,

    /// <summary>
    /// Low severity - monitor situation
    /// </summary>
    Low = 2,

    /// <summary>
    /// Medium severity - attention needed
    /// </summary>
    Medium = 3,

    /// <summary>
    /// High severity - prompt action required
    /// </summary>
    High = 4,

    /// <summary>
    /// Critical - immediate action required
    /// </summary>
    Critical = 5
}
