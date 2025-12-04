namespace CoralLedger.Domain.Enums;

/// <summary>
/// Channels for alert notifications (flags enum for multiple selections)
/// </summary>
[Flags]
public enum NotificationChannel
{
    None = 0,

    /// <summary>
    /// Show in dashboard/UI
    /// </summary>
    Dashboard = 1,

    /// <summary>
    /// Send email notification
    /// </summary>
    Email = 2,

    /// <summary>
    /// Send push notification (PWA)
    /// </summary>
    Push = 4,

    /// <summary>
    /// Send via SignalR real-time
    /// </summary>
    RealTime = 8,

    /// <summary>
    /// All channels
    /// </summary>
    All = Dashboard | Email | Push | RealTime
}
