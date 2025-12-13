using CoralLedger.Blue.Domain.Enums;

namespace CoralLedger.Blue.Application.Common.Interfaces;

/// <summary>
/// AI service for natural language queries about marine data
/// Sprint 5.1: Enhanced with query interpretation and disambiguation (US-5.1.2, US-5.1.4)
/// </summary>
public interface IMarineAIService
{
    /// <summary>
    /// Whether AI features are configured and available
    /// </summary>
    bool IsConfigured { get; }

    /// <summary>
    /// Interpret a natural language query and return what the AI understood (US-5.1.2)
    /// This allows users to confirm the interpretation before execution
    /// </summary>
    Task<QueryInterpretation> InterpretQueryAsync(
        string naturalLanguageQuery,
        UserPersona persona = UserPersona.General,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute a previously interpreted query
    /// </summary>
    Task<MarineQueryResult> ExecuteInterpretedQueryAsync(
        string interpretationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Process a natural language query about marine data
    /// </summary>
    Task<MarineQueryResult> QueryAsync(
        string naturalLanguageQuery,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Process a natural language query with persona-aware formatting
    /// </summary>
    Task<MarineQueryResult> QueryAsync(
        string naturalLanguageQuery,
        UserPersona persona,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get suggested queries based on context
    /// </summary>
    Task<IReadOnlyList<string>> GetSuggestedQueriesAsync(
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Query interpretation result showing what the AI understood (US-5.1.2)
/// "I understood this as: Reefs with DHW > 4 in Exumas. Correct?"
/// </summary>
public record QueryInterpretation(
    string InterpretationId,
    string OriginalQuery,
    string InterpretedAs,
    IReadOnlyList<string> DataSourcesUsed,
    IReadOnlyList<DisambiguationOption>? DisambiguationNeeded = null,
    bool RequiresDisambiguation = false,
    string? SecurityWarning = null,
    UserPersona Persona = UserPersona.General);

/// <summary>
/// Disambiguation option for vague terms (US-5.1.4)
/// "By 'healthy', do you mean: Low bleaching? High coral cover?"
/// </summary>
public record DisambiguationOption(
    string VagueTerm,
    string Question,
    IReadOnlyList<string> Options);

public record MarineQueryResult(
    bool Success,
    string? Answer = null,
    MarineQueryData? Data = null,
    UserPersona Persona = UserPersona.General,
    string? SqlGenerated = null,
    string? InterpretedAs = null,
    string? Error = null);

public record MarineQueryData(
    string DataType,
    object? Results = null,
    int? Count = null);
