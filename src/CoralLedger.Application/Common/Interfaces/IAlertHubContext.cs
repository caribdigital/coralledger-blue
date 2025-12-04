namespace CoralLedger.Application.Common.Interfaces;

/// <summary>
/// Abstraction for SignalR alert hub operations
/// </summary>
public interface IAlertHubContext
{
    /// <summary>
    /// Send alert to all subscribers
    /// </summary>
    Task SendToAllAsync(object alertData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send alert to MPA-specific subscribers
    /// </summary>
    Task SendToMpaAsync(Guid mpaId, object alertData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send vessel position update
    /// </summary>
    Task SendVesselPositionAsync(object positionData, CancellationToken cancellationToken = default);
}
