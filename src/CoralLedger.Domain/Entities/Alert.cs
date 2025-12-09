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
    public bool IsAcknowledged { get; private set; } = false;

    /// <summary>
    /// Who acknowledged the alert
    /// </summary>
    public string? AcknowledgedBy { get; private set; }

    /// <summary>
    /// When the alert was acknowledged
    /// </summary>
    public DateTime? AcknowledgedAt { get; private set; }

    /// <summary>
    /// When the alert was generated
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the alert expires (for auto-cleanup)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Whether this alert has expired
    /// </summary>
    public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;

    /// <summary>
    /// Creates a new alert
    /// </summary>
    public static Alert Create(
        Guid alertRuleId,
        AlertType type,
        AlertSeverity severity,
        string title,
        string message,
        Point? location = null,
        Guid? marineProtectedAreaId = null,
        Guid? vesselId = null,
        string? data = null,
        TimeSpan? expiresIn = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        return new Alert
        {
            AlertRuleId = alertRuleId,
            Type = type,
            Severity = severity,
            Title = title,
            Message = message,
            Location = location,
            MarineProtectedAreaId = marineProtectedAreaId,
            VesselId = vesselId,
            Data = data,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiresIn.HasValue ? DateTime.UtcNow.Add(expiresIn.Value) : null
        };
    }

    /// <summary>
    /// Acknowledges the alert
    /// </summary>
    /// <param name="acknowledgedBy">User who acknowledged the alert</param>
    /// <exception cref="InvalidOperationException">If alert is already acknowledged or expired</exception>
    public void Acknowledge(string acknowledgedBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(acknowledgedBy);

        if (IsAcknowledged)
        {
            throw new InvalidOperationException("Alert has already been acknowledged.");
        }

        if (IsExpired)
        {
            throw new InvalidOperationException("Cannot acknowledge an expired alert.");
        }

        IsAcknowledged = true;
        AcknowledgedBy = acknowledgedBy;
        AcknowledgedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Escalates the alert to a higher severity level
    /// </summary>
    /// <exception cref="InvalidOperationException">If alert is already at critical level or expired</exception>
    public void Escalate()
    {
        if (IsExpired)
        {
            throw new InvalidOperationException("Cannot escalate an expired alert.");
        }

        if (Severity == AlertSeverity.Critical)
        {
            throw new InvalidOperationException("Alert is already at critical severity.");
        }

        Severity = (AlertSeverity)((int)Severity + 1);
    }

    /// <summary>
    /// Sets the alert expiration time
    /// </summary>
    public void SetExpiration(DateTime expiresAt)
    {
        if (expiresAt <= DateTime.UtcNow)
        {
            throw new ArgumentException("Expiration time must be in the future.", nameof(expiresAt));
        }

        ExpiresAt = expiresAt;
    }
}
