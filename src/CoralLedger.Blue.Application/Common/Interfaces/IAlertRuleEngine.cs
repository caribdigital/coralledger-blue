using CoralLedger.Blue.Domain.Entities;
using CoralLedger.Blue.Domain.Enums;

namespace CoralLedger.Blue.Application.Common.Interfaces;

/// <summary>
/// Engine for evaluating alert rules and generating alerts
/// </summary>
public interface IAlertRuleEngine
{
    /// <summary>
    /// Evaluate all active rules and generate alerts
    /// </summary>
    Task<IReadOnlyList<Alert>> EvaluateAllRulesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Evaluate a specific rule
    /// </summary>
    Task<IReadOnlyList<Alert>> EvaluateRuleAsync(Guid ruleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Evaluate rules of a specific type
    /// </summary>
    Task<IReadOnlyList<Alert>> EvaluateRulesByTypeAsync(AlertType type, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create and persist alerts from evaluation results
    /// </summary>
    Task<int> PersistAlertsAsync(IEnumerable<Alert> alerts, CancellationToken cancellationToken = default);
}
