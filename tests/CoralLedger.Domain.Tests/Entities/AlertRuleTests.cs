using CoralLedger.Domain.Entities;
using CoralLedger.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace CoralLedger.Domain.Tests.Entities;

public class AlertRuleTests
{
    private const string ValidConditions = "{\"minAlertLevel\": 2}";

    [Fact]
    public void Create_WithValidData_SetsAllProperties()
    {
        // Arrange
        var mpaId = Guid.NewGuid();
        var cooldown = TimeSpan.FromMinutes(30);

        // Act
        var rule = AlertRule.Create(
            name: "Bleaching Alert Rule",
            type: AlertType.Bleaching,
            conditions: ValidConditions,
            description: "Monitors for coral bleaching events",
            severity: AlertSeverity.High,
            marineProtectedAreaId: mpaId,
            notificationChannels: NotificationChannel.Dashboard | NotificationChannel.Email,
            notificationEmails: "admin@coralledger.org",
            cooldownPeriod: cooldown);

        // Assert
        rule.Name.Should().Be("Bleaching Alert Rule");
        rule.Type.Should().Be(AlertType.Bleaching);
        rule.Conditions.Should().Be(ValidConditions);
        rule.Description.Should().Be("Monitors for coral bleaching events");
        rule.Severity.Should().Be(AlertSeverity.High);
        rule.MarineProtectedAreaId.Should().Be(mpaId);
        rule.NotificationChannels.Should().Be(NotificationChannel.Dashboard | NotificationChannel.Email);
        rule.NotificationEmails.Should().Be("admin@coralledger.org");
        rule.CooldownPeriod.Should().Be(cooldown);
        rule.IsActive.Should().BeTrue();
        rule.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_GeneratesUniqueId()
    {
        // Act
        var rule1 = AlertRule.Create("Rule 1", AlertType.Bleaching, ValidConditions);
        var rule2 = AlertRule.Create("Rule 2", AlertType.Bleaching, ValidConditions);

        // Assert
        rule1.Id.Should().NotBeEmpty();
        rule2.Id.Should().NotBeEmpty();
        rule1.Id.Should().NotBe(rule2.Id);
    }

    [Fact]
    public void Create_SetsDefaultValuesCorrectly()
    {
        // Act
        var rule = AlertRule.Create("Test Rule", AlertType.VesselInMPA, ValidConditions);

        // Assert
        rule.IsActive.Should().BeTrue();
        rule.Severity.Should().Be(AlertSeverity.Medium);
        rule.NotificationChannels.Should().Be(NotificationChannel.Dashboard);
        rule.CooldownPeriod.Should().Be(TimeSpan.FromHours(1));
        rule.LastTriggeredAt.Should().BeNull();
        rule.UpdatedAt.Should().BeNull();
        rule.Description.Should().BeNull();
        rule.NotificationEmails.Should().BeNull();
        rule.MarineProtectedAreaId.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithNullOrEmptyName_ThrowsArgumentException(string? name)
    {
        // Act & Assert
        var act = () => AlertRule.Create(name!, AlertType.Bleaching, ValidConditions);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithNullOrEmptyConditions_ThrowsArgumentException(string? conditions)
    {
        // Act & Assert
        var act = () => AlertRule.Create("Test Rule", AlertType.Bleaching, conditions!);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(AlertType.Bleaching)]
    [InlineData(AlertType.FishingActivity)]
    [InlineData(AlertType.VesselInMPA)]
    [InlineData(AlertType.VesselDarkEvent)]
    [InlineData(AlertType.CitizenObservation)]
    [InlineData(AlertType.TemperatureAnomaly)]
    [InlineData(AlertType.DegreeHeatingWeek)]
    [InlineData(AlertType.System)]
    public void Create_SupportsAllAlertTypes(AlertType alertType)
    {
        // Act
        var rule = AlertRule.Create("Test Rule", alertType, ValidConditions);

        // Assert
        rule.Type.Should().Be(alertType);
    }

    [Fact]
    public void Activate_WhenInactive_SetsIsActiveTrue()
    {
        // Arrange
        var rule = AlertRule.Create("Test Rule", AlertType.Bleaching, ValidConditions);
        rule.Deactivate();
        rule.IsActive.Should().BeFalse();

        // Act
        rule.Activate();

        // Assert
        rule.IsActive.Should().BeTrue();
        rule.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Activate_WhenAlreadyActive_DoesNothing()
    {
        // Arrange
        var rule = AlertRule.Create("Test Rule", AlertType.Bleaching, ValidConditions);
        rule.IsActive.Should().BeTrue();
        var originalUpdatedAt = rule.UpdatedAt;

        // Act
        rule.Activate();

        // Assert
        rule.IsActive.Should().BeTrue();
        rule.UpdatedAt.Should().Be(originalUpdatedAt);
    }

    [Fact]
    public void Deactivate_WhenActive_SetsIsActiveFalse()
    {
        // Arrange
        var rule = AlertRule.Create("Test Rule", AlertType.Bleaching, ValidConditions);
        rule.IsActive.Should().BeTrue();

        // Act
        rule.Deactivate();

        // Assert
        rule.IsActive.Should().BeFalse();
        rule.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_DoesNothing()
    {
        // Arrange
        var rule = AlertRule.Create("Test Rule", AlertType.Bleaching, ValidConditions);
        rule.Deactivate();
        var originalUpdatedAt = rule.UpdatedAt;

        // Act
        rule.Deactivate();

        // Assert
        rule.IsActive.Should().BeFalse();
        rule.UpdatedAt.Should().Be(originalUpdatedAt);
    }

    [Fact]
    public void UpdateConditions_SetsNewConditions()
    {
        // Arrange
        var rule = AlertRule.Create("Test Rule", AlertType.Bleaching, ValidConditions);
        var newConditions = "{\"minAlertLevel\": 4, \"minDhw\": 8}";

        // Act
        rule.UpdateConditions(newConditions);

        // Assert
        rule.Conditions.Should().Be(newConditions);
        rule.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void UpdateConditions_WithNullOrEmpty_ThrowsArgumentException(string? conditions)
    {
        // Arrange
        var rule = AlertRule.Create("Test Rule", AlertType.Bleaching, ValidConditions);

        // Act & Assert
        var act = () => rule.UpdateConditions(conditions!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RecordTrigger_SetsLastTriggeredAt()
    {
        // Arrange
        var rule = AlertRule.Create("Test Rule", AlertType.Bleaching, ValidConditions);
        rule.LastTriggeredAt.Should().BeNull();

        // Act
        rule.RecordTrigger();

        // Assert
        rule.LastTriggeredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        rule.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void CanTrigger_WhenActiveAndNeverTriggered_ReturnsTrue()
    {
        // Arrange
        var rule = AlertRule.Create("Test Rule", AlertType.Bleaching, ValidConditions);

        // Act & Assert
        rule.CanTrigger().Should().BeTrue();
    }

    [Fact]
    public void CanTrigger_WhenInactive_ReturnsFalse()
    {
        // Arrange
        var rule = AlertRule.Create("Test Rule", AlertType.Bleaching, ValidConditions);
        rule.Deactivate();

        // Act & Assert
        rule.CanTrigger().Should().BeFalse();
    }

    [Fact]
    public void CanTrigger_WhenCooldownNotElapsed_ReturnsFalse()
    {
        // Arrange
        var rule = AlertRule.Create(
            "Test Rule",
            AlertType.Bleaching,
            ValidConditions,
            cooldownPeriod: TimeSpan.FromHours(1));
        rule.RecordTrigger();

        // Act & Assert
        rule.CanTrigger().Should().BeFalse();
    }

    [Fact]
    public void CanTrigger_WhenCooldownElapsed_ReturnsTrue()
    {
        // Arrange - use very short cooldown for testing
        var rule = AlertRule.Create(
            "Test Rule",
            AlertType.Bleaching,
            ValidConditions,
            cooldownPeriod: TimeSpan.FromMilliseconds(1));
        rule.RecordTrigger();

        // Wait for cooldown
        Thread.Sleep(10);

        // Act & Assert
        rule.CanTrigger().Should().BeTrue();
    }

    [Fact]
    public void UpdateNotificationSettings_UpdatesAllSettings()
    {
        // Arrange
        var rule = AlertRule.Create("Test Rule", AlertType.Bleaching, ValidConditions);
        var newChannels = NotificationChannel.Email | NotificationChannel.Push;
        var newEmails = "alerts@coralledger.org";
        var newCooldown = TimeSpan.FromMinutes(15);

        // Act
        rule.UpdateNotificationSettings(newChannels, newEmails, newCooldown);

        // Assert
        rule.NotificationChannels.Should().Be(newChannels);
        rule.NotificationEmails.Should().Be(newEmails);
        rule.CooldownPeriod.Should().Be(newCooldown);
        rule.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void UpdateNotificationSettings_WithEmailChannelButNoEmails_ThrowsArgumentException()
    {
        // Arrange
        var rule = AlertRule.Create("Test Rule", AlertType.Bleaching, ValidConditions);

        // Act & Assert
        var act = () => rule.UpdateNotificationSettings(NotificationChannel.Email, emails: null);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Email addresses required when Email channel is enabled.*");
    }

    [Fact]
    public void UpdateNotificationSettings_WithNegativeCooldown_ThrowsArgumentException()
    {
        // Arrange
        var rule = AlertRule.Create("Test Rule", AlertType.Bleaching, ValidConditions);

        // Act & Assert
        var act = () => rule.UpdateNotificationSettings(
            NotificationChannel.Dashboard,
            cooldownPeriod: TimeSpan.FromHours(-1));
        act.Should().Throw<ArgumentException>()
            .WithMessage("Cooldown period cannot be negative.*");
    }

    [Fact]
    public void UpdateNotificationSettings_WithoutCooldown_KeepsExisting()
    {
        // Arrange
        var originalCooldown = TimeSpan.FromMinutes(45);
        var rule = AlertRule.Create(
            "Test Rule",
            AlertType.Bleaching,
            ValidConditions,
            cooldownPeriod: originalCooldown);

        // Act
        rule.UpdateNotificationSettings(NotificationChannel.Push);

        // Assert
        rule.CooldownPeriod.Should().Be(originalCooldown);
    }

    [Fact]
    public void UpdateSeverity_UpdatesSeverityLevel()
    {
        // Arrange
        var rule = AlertRule.Create("Test Rule", AlertType.Bleaching, ValidConditions);
        rule.Severity.Should().Be(AlertSeverity.Medium);

        // Act
        rule.UpdateSeverity(AlertSeverity.Critical);

        // Assert
        rule.Severity.Should().Be(AlertSeverity.Critical);
        rule.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData(AlertSeverity.Info)]
    [InlineData(AlertSeverity.Low)]
    [InlineData(AlertSeverity.Medium)]
    [InlineData(AlertSeverity.High)]
    [InlineData(AlertSeverity.Critical)]
    public void UpdateSeverity_SupportsAllSeverityLevels(AlertSeverity severity)
    {
        // Arrange
        var rule = AlertRule.Create("Test Rule", AlertType.Bleaching, ValidConditions);

        // Act
        rule.UpdateSeverity(severity);

        // Assert
        rule.Severity.Should().Be(severity);
    }

    [Theory]
    [InlineData(NotificationChannel.None)]
    [InlineData(NotificationChannel.Dashboard)]
    [InlineData(NotificationChannel.Push)]
    [InlineData(NotificationChannel.RealTime)]
    [InlineData(NotificationChannel.Dashboard | NotificationChannel.Push)]
    [InlineData(NotificationChannel.All)]
    public void UpdateNotificationSettings_SupportsAllChannelCombinations(NotificationChannel channels)
    {
        // Arrange
        var rule = AlertRule.Create("Test Rule", AlertType.Bleaching, ValidConditions);
        var emails = channels.HasFlag(NotificationChannel.Email) ? "test@test.com" : null;

        // Act
        rule.UpdateNotificationSettings(channels, emails);

        // Assert
        rule.NotificationChannels.Should().Be(channels);
    }

    [Fact]
    public void Create_WithCustomCooldown_SetsCooldownPeriod()
    {
        // Arrange
        var customCooldown = TimeSpan.FromMinutes(15);

        // Act
        var rule = AlertRule.Create(
            "Test Rule",
            AlertType.Bleaching,
            ValidConditions,
            cooldownPeriod: customCooldown);

        // Assert
        rule.CooldownPeriod.Should().Be(customCooldown);
    }
}
