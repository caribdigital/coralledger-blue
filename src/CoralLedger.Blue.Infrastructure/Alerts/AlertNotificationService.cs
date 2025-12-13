using CoralLedger.Blue.Application.Common.Interfaces;
using CoralLedger.Blue.Domain.Entities;
using CoralLedger.Blue.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace CoralLedger.Blue.Infrastructure.Alerts;

/// <summary>
/// Service for sending alert notifications through configured channels
/// </summary>
public class AlertNotificationService : IAlertNotificationService
{
    private readonly IAlertHubContext _hubContext;
    private readonly ILogger<AlertNotificationService> _logger;

    public AlertNotificationService(
        IAlertHubContext hubContext,
        ILogger<AlertNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendNotificationAsync(Alert alert, AlertRule rule, CancellationToken cancellationToken = default)
    {
        var channels = rule.NotificationChannels;

        // Real-time via SignalR
        if (channels.HasFlag(NotificationChannel.RealTime) || channels.HasFlag(NotificationChannel.Dashboard))
        {
            await SendRealTimeNotificationAsync(alert, cancellationToken);
        }

        // Email notification (placeholder - would integrate with email service)
        if (channels.HasFlag(NotificationChannel.Email) && !string.IsNullOrEmpty(rule.NotificationEmails))
        {
            _logger.LogInformation("Would send email to {Emails} for alert {AlertId}",
                rule.NotificationEmails, alert.Id);
            // TODO: Integrate with email service (SendGrid, SMTP, etc.)
        }

        // Push notification (placeholder - would integrate with web push)
        if (channels.HasFlag(NotificationChannel.Push))
        {
            _logger.LogInformation("Would send push notification for alert {AlertId}", alert.Id);
            // TODO: Integrate with web push service
        }
    }

    public async Task SendRealTimeNotificationAsync(Alert alert, CancellationToken cancellationToken = default)
    {
        var alertData = new
        {
            id = alert.Id,
            type = alert.Type.ToString(),
            severity = alert.Severity.ToString(),
            title = alert.Title,
            message = alert.Message,
            mpaId = alert.MarineProtectedAreaId,
            vesselId = alert.VesselId,
            createdAt = alert.CreatedAt,
            location = alert.Location != null ? new { lon = alert.Location.X, lat = alert.Location.Y } : null
        };

        // Send to all alert subscribers
        await _hubContext.SendToAllAsync(alertData, cancellationToken);

        // Send to MPA-specific subscribers
        if (alert.MarineProtectedAreaId.HasValue)
        {
            await _hubContext.SendToMpaAsync(alert.MarineProtectedAreaId.Value, alertData, cancellationToken);
        }

        _logger.LogInformation("Sent real-time alert: {Title}", alert.Title);
    }
}
