namespace CoralLedger.Blue.Application.Common.Interfaces;

/// <summary>
/// Service for sending web push notifications
/// </summary>
public interface IPushNotificationService
{
    /// <summary>
    /// Send a push notification to all subscribed clients
    /// </summary>
    Task<int> SendToAllAsync(
        string title,
        string message,
        string? url = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send a push notification to a specific subscription
    /// </summary>
    Task<bool> SendToSubscriptionAsync(
        PushSubscription subscription,
        string title,
        string message,
        string? url = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Register a push subscription
    /// </summary>
    Task RegisterSubscriptionAsync(
        PushSubscription subscription,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove a push subscription
    /// </summary>
    Task UnregisterSubscriptionAsync(
        string endpoint,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get VAPID public key for client subscription
    /// </summary>
    string GetVapidPublicKey();
}

/// <summary>
/// Represents a web push subscription from a client
/// </summary>
public record PushSubscription(
    string Endpoint,
    string P256dh,
    string Auth,
    string? UserId = null);
