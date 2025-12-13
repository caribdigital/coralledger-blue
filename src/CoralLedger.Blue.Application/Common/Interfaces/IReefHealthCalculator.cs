using CoralLedger.Blue.Domain.Enums;

namespace CoralLedger.Blue.Application.Common.Interfaces;

/// <summary>
/// Calculates reef health based on multiple data sources including:
/// - NOAA Coral Reef Watch bleaching data (DHW, SST anomaly)
/// - Survey data (coral cover, bleaching percentage)
/// - Citizen observations (coral bleaching reports, severity)
/// - Fishing pressure (nearby fishing activity)
/// </summary>
public interface IReefHealthCalculator
{
    /// <summary>
    /// Calculate overall reef health score (0-100) and status
    /// </summary>
    Task<ReefHealthAssessment> CalculateHealthAsync(Guid reefId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate health for all reefs in an MPA
    /// </summary>
    Task<IEnumerable<ReefHealthAssessment>> CalculateMpaReefHealthAsync(Guid mpaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate health based on provided metrics (for testing or offline calculation)
    /// </summary>
    ReefHealthAssessment CalculateFromMetrics(ReefHealthMetrics metrics);
}

/// <summary>
/// Comprehensive reef health assessment result
/// </summary>
public record ReefHealthAssessment
{
    public Guid ReefId { get; init; }
    public string ReefName { get; init; } = string.Empty;
    public ReefHealth HealthStatus { get; init; }
    public double OverallScore { get; init; }  // 0-100
    public DateTime AssessmentTime { get; init; }

    // Component scores (0-100, higher is better)
    public double BleachingScore { get; init; }       // Based on DHW and bleaching percentage
    public double CoralCoverScore { get; init; }      // Based on live coral coverage
    public double ObservationScore { get; init; }     // Based on citizen reports
    public double FishingPressureScore { get; init; } // Based on nearby fishing activity

    // Data freshness indicators
    public bool HasRecentSurvey { get; init; }       // Survey within 6 months
    public bool HasRecentBleachingData { get; init; } // NOAA data within 7 days
    public bool HasCitizenReports { get; init; }      // Reports within 30 days

    // Alerts
    public IReadOnlyList<string> Alerts { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Recommendations { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Input metrics for reef health calculation
/// </summary>
public record ReefHealthMetrics
{
    // NOAA Coral Reef Watch data
    public double? DegreeHeatingWeek { get; init; }
    public double? SstAnomaly { get; init; }
    public BleachingAlertLevel? BleachingAlertLevel { get; init; }

    // Survey data
    public double? CoralCoverPercentage { get; init; }
    public double? BleachingPercentage { get; init; }
    public DateOnly? LastSurveyDate { get; init; }

    // Citizen observations (aggregated)
    public int RecentObservationCount { get; init; }  // Last 30 days
    public double? AverageSeverity { get; init; }     // 1-5 scale
    public int BleachingReportCount { get; init; }
    public int DebrisReportCount { get; init; }

    // Fishing pressure
    public int FishingEventsNearby7Days { get; init; }
    public int FishingEventsNearby30Days { get; init; }
    public double NearestFishingDistanceKm { get; init; } = double.MaxValue;
}
