using CoralLedger.Blue.Domain.Common;
using CoralLedger.Blue.Domain.Enums;

namespace CoralLedger.Blue.Domain.Entities;

/// <summary>
/// Audit log for Natural Language Query (NLQ) system
/// Sprint 5.1: Captures all queries and generated SQL for security and debugging
/// </summary>
public class NLQAuditLog : BaseEntity
{
    public string OriginalQuery { get; private set; } = string.Empty;
    public string? InterpretedAs { get; private set; }
    public string? GeneratedSql { get; private set; }
    public UserPersona Persona { get; private set; }
    public NLQQueryStatus Status { get; private set; }
    public DateTime QueryTime { get; private set; }
    public int? ResponseTimeMs { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? DataSourcesUsed { get; private set; }
    public bool RequiredDisambiguation { get; private set; }
    public bool SecurityRestrictionApplied { get; private set; }
    public string? UserIp { get; private set; }
    public string? UserAgent { get; private set; }

    private NLQAuditLog() { }

    public static NLQAuditLog Create(
        string originalQuery,
        UserPersona persona,
        string? userIp = null,
        string? userAgent = null)
    {
        return new NLQAuditLog
        {
            OriginalQuery = originalQuery,
            Persona = persona,
            Status = NLQQueryStatus.Pending,
            QueryTime = DateTime.UtcNow,
            UserIp = userIp,
            UserAgent = userAgent
        };
    }

    public void MarkInterpreted(
        string interpretedAs,
        IEnumerable<string> dataSourcesUsed,
        bool requiredDisambiguation = false)
    {
        InterpretedAs = interpretedAs;
        DataSourcesUsed = string.Join(", ", dataSourcesUsed);
        RequiredDisambiguation = requiredDisambiguation;
        Status = NLQQueryStatus.Interpreted;
    }

    public void MarkExecuted(
        string? generatedSql,
        int responseTimeMs,
        bool securityRestrictionApplied = false)
    {
        GeneratedSql = generatedSql;
        ResponseTimeMs = responseTimeMs;
        SecurityRestrictionApplied = securityRestrictionApplied;
        Status = NLQQueryStatus.Completed;
    }

    public void MarkFailed(string errorMessage, int responseTimeMs)
    {
        ErrorMessage = errorMessage;
        ResponseTimeMs = responseTimeMs;
        Status = NLQQueryStatus.Failed;
    }

    public void MarkSecurityBlocked(string reason)
    {
        ErrorMessage = reason;
        SecurityRestrictionApplied = true;
        Status = NLQQueryStatus.SecurityBlocked;
    }
}

public enum NLQQueryStatus
{
    Pending,
    Interpreted,
    Completed,
    Failed,
    SecurityBlocked
}
