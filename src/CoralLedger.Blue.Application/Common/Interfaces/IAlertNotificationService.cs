using CoralLedger.Blue.Domain.Entities;

namespace CoralLedger.Blue.Application.Common.Interfaces;

/// <summary>
/// Service for sending alert notifications
/// </summary>
public interface IAlertNotificationService
{
    /// <summary>
    /// Send notification for an alert through configured channels
    /// </summary>
    Task SendNotificationAsync(Alert alert, AlertRule rule, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send real-time notification via SignalR
    /// </summary>
    Task SendRealTimeNotificationAsync(Alert alert, CancellationToken cancellationToken = default);
}
