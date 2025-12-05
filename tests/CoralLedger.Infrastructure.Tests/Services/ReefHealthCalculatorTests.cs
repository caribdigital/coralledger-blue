using CoralLedger.Application.Common.Interfaces;
using CoralLedger.Domain.Enums;
using CoralLedger.Infrastructure.Data;
using CoralLedger.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace CoralLedger.Infrastructure.Tests.Services;

public class ReefHealthCalculatorTests
{
    private readonly ReefHealthCalculator _calculator;
    private readonly Mock<IDateTimeService> _dateTimeService;

    public ReefHealthCalculatorTests()
    {
        _dateTimeService = new Mock<IDateTimeService>();
        _dateTimeService.Setup(x => x.UtcNow).Returns(DateTime.UtcNow);

        // For unit tests of CalculateFromMetrics, we don't need a real DbContext
        // We'll use null and only test the calculation logic
        _calculator = new ReefHealthCalculator(
            null!, // DbContext not needed for CalculateFromMetrics
            NullLogger<ReefHealthCalculator>.Instance,
            _dateTimeService.Object);
    }

    #region Overall Score Calculation Tests

    [Fact]
    public void CalculateFromMetrics_WithExcellentMetrics_ReturnsExcellentHealth()
    {
        // Arrange - Perfect reef conditions
        var metrics = new ReefHealthMetrics
        {
            DegreeHeatingWeek = 0,
            SstAnomaly = 0.2,
            CoralCoverPercentage = 45,
            BleachingPercentage = 0,
            LastSurveyDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)),
            RecentObservationCount = 0,
            FishingEventsNearby7Days = 0,
            FishingEventsNearby30Days = 0
        };

        // Act
        var result = _calculator.CalculateFromMetrics(metrics);

        // Assert
        result.HealthStatus.Should().Be(ReefHealth.Excellent);
        result.OverallScore.Should().BeGreaterThanOrEqualTo(85);
        result.Alerts.Should().BeEmpty();
    }

    [Fact]
    public void CalculateFromMetrics_WithCriticalDHW_ReturnsCriticalHealth()
    {
        // Arrange - Extreme bleaching stress (DHW >= 16)
        var metrics = new ReefHealthMetrics
        {
            DegreeHeatingWeek = 18,
            CoralCoverPercentage = 25
        };

        // Act
        var result = _calculator.CalculateFromMetrics(metrics);

        // Assert
        result.HealthStatus.Should().Be(ReefHealth.Critical);
        result.Alerts.Should().Contain(a => a.Contains("CRITICAL"));
        result.Alerts.Should().Contain(a => a.Contains("mass mortality"));
    }

    [Fact]
    public void CalculateFromMetrics_WithLowCoralCover_ReturnsCriticalHealth()
    {
        // Arrange - Very low coral cover (< 5%)
        var metrics = new ReefHealthMetrics
        {
            DegreeHeatingWeek = 0,
            CoralCoverPercentage = 3
        };

        // Act
        var result = _calculator.CalculateFromMetrics(metrics);

        // Assert
        result.HealthStatus.Should().Be(ReefHealth.Critical);
        result.Alerts.Should().Contain(a => a.Contains("Critical coral cover"));
    }

    #endregion

    #region Bleaching Score Tests

    [Theory]
    [InlineData(0, 100)]     // No stress
    [InlineData(2, 95)]      // Minor stress (DHW 0-4)
    [InlineData(5, 80)]      // Watch level (DHW >= 4)
    [InlineData(10, 60)]     // Alert level 2 (DHW >= 8)
    [InlineData(18, 40)]     // Alert level 4 (DHW >= 16)
    public void CalculateFromMetrics_DHWAffectsBleachingScore(double dhw, double expectedMinScore)
    {
        // Arrange
        var metrics = new ReefHealthMetrics
        {
            DegreeHeatingWeek = dhw,
            CoralCoverPercentage = 30 // Good baseline
        };

        // Act
        var result = _calculator.CalculateFromMetrics(metrics);

        // Assert
        result.BleachingScore.Should().BeLessThanOrEqualTo(expectedMinScore);
    }

    [Theory]
    [InlineData(0.3, 0)]     // Low anomaly - no deduction
    [InlineData(1.2, 8)]     // Moderate anomaly
    [InlineData(2.5, 15)]    // High anomaly
    public void CalculateFromMetrics_SstAnomalyAffectsBleachingScore(double anomaly, int expectedDeduction)
    {
        // Arrange
        var metrics = new ReefHealthMetrics
        {
            DegreeHeatingWeek = 0,
            SstAnomaly = anomaly,
            CoralCoverPercentage = 30
        };

        // Act
        var result = _calculator.CalculateFromMetrics(metrics);

        // Assert
        // With DHW=0, base bleaching score is 100. Anomaly reduces it.
        result.BleachingScore.Should().BeLessThanOrEqualTo(100 - expectedDeduction + 5); // Allow some tolerance
    }

    [Fact]
    public void CalculateFromMetrics_HighBleachingPercentage_TriggersAlert()
    {
        // Arrange - 50% bleaching observed in survey
        var metrics = new ReefHealthMetrics
        {
            DegreeHeatingWeek = 6,
            BleachingPercentage = 55,
            CoralCoverPercentage = 20
        };

        // Act
        var result = _calculator.CalculateFromMetrics(metrics);

        // Assert
        result.Alerts.Should().Contain(a => a.Contains("Survey shows") && a.Contains("bleaching"));
        result.Recommendations.Should().Contain(r => r.Contains("emergency coral restoration"));
    }

    #endregion

    #region Coral Cover Score Tests

    [Theory]
    [InlineData(45, 100)]    // Excellent (>= 40%)
    [InlineData(30, 85)]     // Good (25-40%)
    [InlineData(18, 65)]     // Fair (15-25%)
    [InlineData(8, 40)]      // Poor (5-15%)
    [InlineData(2, 20)]      // Critical (< 5%)
    public void CalculateFromMetrics_CoralCoverAffectsScore(double coralCover, double expectedScore)
    {
        // Arrange
        var metrics = new ReefHealthMetrics
        {
            DegreeHeatingWeek = 0,
            CoralCoverPercentage = coralCover
        };

        // Act
        var result = _calculator.CalculateFromMetrics(metrics);

        // Assert
        result.CoralCoverScore.Should().Be(expectedScore);
    }

    [Fact]
    public void CalculateFromMetrics_NoCoralCoverData_ReturnsNeutralScore()
    {
        // Arrange - No survey data available
        var metrics = new ReefHealthMetrics
        {
            DegreeHeatingWeek = 0,
            CoralCoverPercentage = null
        };

        // Act
        var result = _calculator.CalculateFromMetrics(metrics);

        // Assert
        result.CoralCoverScore.Should().Be(50); // Neutral when no data
    }

    #endregion

    #region Citizen Observation Score Tests

    [Fact]
    public void CalculateFromMetrics_NoObservations_ReturnsNeutralScore()
    {
        // Arrange
        var metrics = new ReefHealthMetrics
        {
            RecentObservationCount = 0,
            CoralCoverPercentage = 30
        };

        // Act
        var result = _calculator.CalculateFromMetrics(metrics);

        // Assert
        result.ObservationScore.Should().Be(50); // Neutral when no observations
    }

    [Fact]
    public void CalculateFromMetrics_BleachingReports_ReduceScore()
    {
        // Arrange - Multiple bleaching reports from citizens
        var metrics = new ReefHealthMetrics
        {
            RecentObservationCount = 5,
            BleachingReportCount = 4,
            AverageSeverity = 4,
            CoralCoverPercentage = 30
        };

        // Act
        var result = _calculator.CalculateFromMetrics(metrics);

        // Assert
        result.ObservationScore.Should().BeLessThan(80);
        result.Alerts.Should().Contain(a => a.Contains("citizen bleaching reports"));
    }

    [Theory]
    [InlineData(1, 100)]  // Very low severity (good)
    [InlineData(3, 100)]  // Neutral severity
    [InlineData(5, 80)]   // High severity (bad)
    public void CalculateFromMetrics_SeverityAffectsObservationScore(double severity, double expectedMaxScore)
    {
        // Arrange
        var metrics = new ReefHealthMetrics
        {
            RecentObservationCount = 3,
            AverageSeverity = severity,
            BleachingReportCount = 0,
            DebrisReportCount = 0,
            CoralCoverPercentage = 30
        };

        // Act
        var result = _calculator.CalculateFromMetrics(metrics);

        // Assert
        result.ObservationScore.Should().BeLessThanOrEqualTo(expectedMaxScore);
    }

    #endregion

    #region Fishing Pressure Score Tests

    [Fact]
    public void CalculateFromMetrics_NoFishingActivity_ReturnsFullScore()
    {
        // Arrange
        var metrics = new ReefHealthMetrics
        {
            FishingEventsNearby7Days = 0,
            FishingEventsNearby30Days = 0,
            NearestFishingDistanceKm = double.MaxValue,
            CoralCoverPercentage = 30
        };

        // Act
        var result = _calculator.CalculateFromMetrics(metrics);

        // Assert
        result.FishingPressureScore.Should().Be(100);
    }

    [Fact]
    public void CalculateFromMetrics_HighFishingActivity_TriggersAlert()
    {
        // Arrange - Heavy fishing activity near reef
        var metrics = new ReefHealthMetrics
        {
            FishingEventsNearby7Days = 8,
            FishingEventsNearby30Days = 20,
            NearestFishingDistanceKm = 0.5,
            CoralCoverPercentage = 30
        };

        // Act
        var result = _calculator.CalculateFromMetrics(metrics);

        // Assert
        result.FishingPressureScore.Should().BeLessThan(50);
        result.Alerts.Should().Contain(a => a.Contains("High fishing pressure"));
        result.Recommendations.Should().Contain(r => r.Contains("fishing regulations"));
    }

    [Theory]
    [InlineData(0.5, 15)]  // Very close - high impact
    [InlineData(1.5, 8)]   // Close - moderate impact
    [InlineData(3.0, 0)]   // Further away - no proximity penalty
    public void CalculateFromMetrics_FishingProximityAffectsScore(double distanceKm, int expectedDeduction)
    {
        // Arrange
        var metrics = new ReefHealthMetrics
        {
            FishingEventsNearby7Days = 0,
            FishingEventsNearby30Days = 0,
            NearestFishingDistanceKm = distanceKm,
            CoralCoverPercentage = 30
        };

        // Act
        var result = _calculator.CalculateFromMetrics(metrics);

        // Assert
        result.FishingPressureScore.Should().Be(100 - expectedDeduction);
    }

    #endregion

    #region Health Status Determination Tests

    [Fact]
    public void CalculateFromMetrics_ExcellentScore_ReturnsExcellentStatus()
    {
        // Arrange - Excellent conditions (score >= 85)
        var metrics = new ReefHealthMetrics
        {
            DegreeHeatingWeek = 0,
            CoralCoverPercentage = 45,
            RecentObservationCount = 0,
            FishingEventsNearby7Days = 0
        };

        // Act
        var result = _calculator.CalculateFromMetrics(metrics);

        // Assert
        result.HealthStatus.Should().Be(ReefHealth.Excellent);
        result.OverallScore.Should().BeGreaterThanOrEqualTo(85);
    }

    [Fact]
    public void CalculateFromMetrics_GoodScore_ReturnsGoodStatus()
    {
        // Arrange - Good conditions (score 70-85)
        var metrics = new ReefHealthMetrics
        {
            DegreeHeatingWeek = 3,
            CoralCoverPercentage = 30,
            RecentObservationCount = 0,
            FishingEventsNearby7Days = 1
        };

        // Act
        var result = _calculator.CalculateFromMetrics(metrics);

        // Assert
        result.HealthStatus.Should().Be(ReefHealth.Good);
        result.OverallScore.Should().BeInRange(70, 85);
    }

    [Fact]
    public void CalculateFromMetrics_FairScore_ReturnsFairStatus()
    {
        // Arrange - Fair conditions (score 50-70)
        // Need higher stress levels to bring score down
        var metrics = new ReefHealthMetrics
        {
            DegreeHeatingWeek = 9,          // High DHW
            SstAnomaly = 1.5,               // Elevated anomaly
            CoralCoverPercentage = 12,      // Low coral cover
            BleachingPercentage = 20,       // Some bleaching observed
            RecentObservationCount = 5,
            BleachingReportCount = 2,
            AverageSeverity = 4,
            FishingEventsNearby7Days = 3,
            NearestFishingDistanceKm = 1.0
        };

        // Act
        var result = _calculator.CalculateFromMetrics(metrics);

        // Assert
        result.HealthStatus.Should().Be(ReefHealth.Fair);
        result.OverallScore.Should().BeInRange(50, 70);
    }

    [Fact]
    public void CalculateFromMetrics_PoorScore_ReturnsPoorStatus()
    {
        // Arrange - Poor conditions (score 30-50)
        // Need severe stress to bring score down to Poor range
        var metrics = new ReefHealthMetrics
        {
            DegreeHeatingWeek = 12,         // Very high DHW
            SstAnomaly = 2.5,               // High anomaly
            CoralCoverPercentage = 6,       // Very low coral cover
            BleachingPercentage = 40,       // Significant bleaching
            RecentObservationCount = 8,
            BleachingReportCount = 5,
            AverageSeverity = 5,
            FishingEventsNearby7Days = 6,
            NearestFishingDistanceKm = 0.5
        };

        // Act
        var result = _calculator.CalculateFromMetrics(metrics);

        // Assert
        result.HealthStatus.Should().Be(ReefHealth.Poor);
        result.OverallScore.Should().BeInRange(30, 50);
    }

    [Fact]
    public void CalculateFromMetrics_CriticalScore_ReturnsCriticalStatus()
    {
        // Arrange - Critical conditions (score < 30 OR DHW >= 16 OR coral < 5%)
        var metrics = new ReefHealthMetrics
        {
            DegreeHeatingWeek = 18,
            CoralCoverPercentage = 10,
            RecentObservationCount = 0,
            FishingEventsNearby7Days = 0
        };

        // Act
        var result = _calculator.CalculateFromMetrics(metrics);

        // Assert
        result.HealthStatus.Should().Be(ReefHealth.Critical);
    }

    #endregion

    #region Data Freshness Tests

    [Fact]
    public void CalculateFromMetrics_RecentSurvey_SetsHasRecentSurveyTrue()
    {
        // Arrange
        var metrics = new ReefHealthMetrics
        {
            LastSurveyDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-2)),
            CoralCoverPercentage = 30
        };

        // Act
        var result = _calculator.CalculateFromMetrics(metrics);

        // Assert
        result.HasRecentSurvey.Should().BeTrue();
    }

    [Fact]
    public void CalculateFromMetrics_OldSurvey_SetsHasRecentSurveyFalse()
    {
        // Arrange
        var metrics = new ReefHealthMetrics
        {
            LastSurveyDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-8)),
            CoralCoverPercentage = 30
        };

        // Act
        var result = _calculator.CalculateFromMetrics(metrics);

        // Assert
        result.HasRecentSurvey.Should().BeFalse();
        result.Recommendations.Should().Contain(r => r.Contains("Schedule reef survey"));
    }

    [Fact]
    public void CalculateFromMetrics_WithBleachingData_SetsHasRecentBleachingTrue()
    {
        // Arrange
        var metrics = new ReefHealthMetrics
        {
            DegreeHeatingWeek = 4,
            CoralCoverPercentage = 30
        };

        // Act
        var result = _calculator.CalculateFromMetrics(metrics);

        // Assert
        result.HasRecentBleachingData.Should().BeTrue();
    }

    [Fact]
    public void CalculateFromMetrics_WithCitizenReports_SetsHasCitizenReportsTrue()
    {
        // Arrange
        var metrics = new ReefHealthMetrics
        {
            RecentObservationCount = 5,
            CoralCoverPercentage = 30
        };

        // Act
        var result = _calculator.CalculateFromMetrics(metrics);

        // Assert
        result.HasCitizenReports.Should().BeTrue();
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public void CalculateFromMetrics_AllNullMetrics_ReturnsNeutralAssessment()
    {
        // Arrange - No data available
        var metrics = new ReefHealthMetrics();

        // Act
        var result = _calculator.CalculateFromMetrics(metrics);

        // Assert
        result.Should().NotBeNull();
        result.OverallScore.Should().BeGreaterThan(0);
        result.HealthStatus.Should().NotBe(ReefHealth.Unknown);
    }

    [Fact]
    public void CalculateFromMetrics_ExtremeDHW_ScoreDoesNotGoNegative()
    {
        // Arrange - Extreme bleaching stress
        var metrics = new ReefHealthMetrics
        {
            DegreeHeatingWeek = 25,
            SstAnomaly = 5.0,
            BleachingPercentage = 80,
            CoralCoverPercentage = 3
        };

        // Act
        var result = _calculator.CalculateFromMetrics(metrics);

        // Assert
        result.BleachingScore.Should().BeGreaterThanOrEqualTo(0);
        result.OverallScore.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void CalculateFromMetrics_ManyFishingEvents_ScoreDoesNotGoNegative()
    {
        // Arrange - Extreme fishing pressure
        var metrics = new ReefHealthMetrics
        {
            FishingEventsNearby7Days = 50,
            FishingEventsNearby30Days = 100,
            NearestFishingDistanceKm = 0.1,
            CoralCoverPercentage = 30
        };

        // Act
        var result = _calculator.CalculateFromMetrics(metrics);

        // Assert
        result.FishingPressureScore.Should().BeGreaterThanOrEqualTo(0);
    }

    #endregion

    #region Component Weight Tests

    [Fact]
    public void CalculateFromMetrics_WeightsAreAppliedCorrectly()
    {
        // Arrange - Known values for each component
        var metrics = new ReefHealthMetrics
        {
            DegreeHeatingWeek = 0,           // Bleaching score = 100
            CoralCoverPercentage = 45,       // Coral score = 100
            RecentObservationCount = 0,      // Observation score = 50 (neutral)
            FishingEventsNearby7Days = 0     // Fishing score = 100
        };

        // Act
        var result = _calculator.CalculateFromMetrics(metrics);

        // Assert - Overall should be weighted average
        // Expected: (100*0.35) + (100*0.30) + (50*0.20) + (100*0.15) = 35 + 30 + 10 + 15 = 90
        result.OverallScore.Should().BeApproximately(90, 2);
        result.BleachingScore.Should().Be(100);
        result.CoralCoverScore.Should().Be(100);
        result.ObservationScore.Should().Be(50);
        result.FishingPressureScore.Should().Be(100);
    }

    #endregion
}
