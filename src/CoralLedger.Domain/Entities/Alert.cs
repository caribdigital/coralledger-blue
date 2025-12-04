using CoralLedger.Domain.Common;
using CoralLedger.Domain.Enums;
using NetTopologySuite.Geometries;

namespace CoralLedger.Domain.Entities;

/// <summary>
/// Generated alert from an alert rule trigger
/// </summary>
public class Alert : BaseEntity
{
    /// <summary>
    /// Rule that generated this alert
    /// </summary>
    public required Guid AlertRuleId { get; set; }
    public AlertRule? AlertRule { get; set; }

    /// <summary>
    /// Type of alert (copied from rule for quick filtering)
    /// </summary>
    public required AlertType Type { get; set; }

    /// <summary>
    /// Severity level
    /// </summary>
    public required AlertSeverity Severity { get; set; }

    /// <summary>
    /// Human-readable alert title
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Detailed message about the alert
    /// </summary>
    public required string Message { get; set; }

    /// <summary>
    /// Location where the alert occurred
    /// </summary>
    public Point? Location { get; set; }

    /// <summary>
    /// Associated MPA if applicable
    /// </summary>
    public Guid? MarineProtectedAreaId { get; set; }
    public MarineProtectedArea? MarineProtectedArea { get; set; }

    /// <summary>
    /// Associated vessel if applicable
    /// </summary>
    public Guid? VesselId { get; set; }
    public Vessel? Vessel { get; set; }

    /// <summary>
    /// JSON data payload with additional context
    /// </summary>
    public string? Data { get; set; }

    /// <summary>
    /// Whether this alert has been acknowledged
    /// </summary>
    public bool IsAcknowledged { get; set; } = false;

    /// <summary>
    /// Who acknowledged the alert
    /// </summary>
    public string? AcknowledgedBy { get; set; }

    /// <summary>
    /// When the alert was acknowledged
    /// </summary>
    public DateTime? AcknowledgedAt { get; set; }

    /// <summary>
    /// When the alert was generated
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the alert expires (for auto-cleanup)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
}
