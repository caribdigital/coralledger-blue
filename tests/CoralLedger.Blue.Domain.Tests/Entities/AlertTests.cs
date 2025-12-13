using CoralLedger.Blue.Domain.Entities;
using CoralLedger.Blue.Domain.Enums;
using FluentAssertions;
using NetTopologySuite.Geometries;
using Xunit;

namespace CoralLedger.Blue.Domain.Tests.Entities;

public class AlertTests
{
    private static readonly Guid TestAlertRuleId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_SetsAllProperties()
    {
        // Arrange
        var mpaId = Guid.NewGuid();
        var vesselId = Guid.NewGuid();
        var location = new Point(-77.35, 25.05);
        var data = "{\"dhw\": 4.5}";

        // Act
        var alert = Alert.Create(
            alertRuleId: TestAlertRuleId,
            type: AlertType.Bleaching,
            severity: AlertSeverity.High,
            title: "Bleaching Alert",
            message: "DHW threshold exceeded in Exuma Cays",
            location: location,
            marineProtectedAreaId: mpaId,
            vesselId: vesselId,
            data: data);

        // Assert
        alert.AlertRuleId.Should().Be(TestAlertRuleId);
        alert.Type.Should().Be(AlertType.Bleaching);
        alert.Severity.Should().Be(AlertSeverity.High);
        alert.Title.Should().Be("Bleaching Alert");
        alert.Message.Should().Be("DHW threshold exceeded in Exuma Cays");
        alert.Location.Should().Be(location);
        alert.MarineProtectedAreaId.Should().Be(mpaId);
        alert.VesselId.Should().Be(vesselId);
        alert.Data.Should().Be(data);
        alert.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_GeneratesUniqueId()
    {
        // Act
        var alert1 = Alert.Create(TestAlertRuleId, AlertType.Bleaching, AlertSeverity.Medium, "Alert 1", "Message 1");
        var alert2 = Alert.Create(TestAlertRuleId, AlertType.Bleaching, AlertSeverity.Medium, "Alert 2", "Message 2");

        // Assert
        alert1.Id.Should().NotBeEmpty();
        alert2.Id.Should().NotBeEmpty();
        alert1.Id.Should().NotBe(alert2.Id);
    }

    [Fact]
    public void Create_SetsDefaultValuesCorrectly()
    {
        // Act
        var alert = Alert.Create(TestAlertRuleId, AlertType.VesselInMPA, AlertSeverity.Low, "Test", "Test message");

        // Assert
        alert.IsAcknowledged.Should().BeFalse();
        alert.AcknowledgedBy.Should().BeNull();
        alert.AcknowledgedAt.Should().BeNull();
        alert.ExpiresAt.Should().BeNull();
        alert.Location.Should().BeNull();
        alert.MarineProtectedAreaId.Should().BeNull();
        alert.VesselId.Should().BeNull();
        alert.Data.Should().BeNull();
    }

    [Fact]
    public void Create_WithExpiresIn_SetsExpirationTime()
    {
        // Arrange
        var expiresIn = TimeSpan.FromHours(24);

        // Act
        var alert = Alert.Create(TestAlertRuleId, AlertType.Bleaching, AlertSeverity.Medium, "Test", "Message", expiresIn: expiresIn);

        // Assert
        alert.ExpiresAt.Should().NotBeNull();
        alert.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.Add(expiresIn), TimeSpan.FromSeconds(5));
        alert.IsExpired.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithNullOrEmptyTitle_ThrowsArgumentException(string? title)
    {
        // Act & Assert
        var act = () => Alert.Create(TestAlertRuleId, AlertType.Bleaching, AlertSeverity.Medium, title!, "Message");
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithNullOrEmptyMessage_ThrowsArgumentException(string? message)
    {
        // Act & Assert
        var act = () => Alert.Create(TestAlertRuleId, AlertType.Bleaching, AlertSeverity.Medium, "Title", message!);
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
        var alert = Alert.Create(TestAlertRuleId, alertType, AlertSeverity.Medium, "Test", "Message");

        // Assert
        alert.Type.Should().Be(alertType);
    }

    [Theory]
    [InlineData(AlertSeverity.Info)]
    [InlineData(AlertSeverity.Low)]
    [InlineData(AlertSeverity.Medium)]
    [InlineData(AlertSeverity.High)]
    [InlineData(AlertSeverity.Critical)]
    public void Create_SupportsAllSeverityLevels(AlertSeverity severity)
    {
        // Act
        var alert = Alert.Create(TestAlertRuleId, AlertType.Bleaching, severity, "Test", "Message");

        // Assert
        alert.Severity.Should().Be(severity);
    }

    [Fact]
    public void Acknowledge_SetsAcknowledgementProperties()
    {
        // Arrange
        var alert = Alert.Create(TestAlertRuleId, AlertType.Bleaching, AlertSeverity.High, "Test", "Message");
        var acknowledgedBy = "admin@coralledger.org";

        // Act
        alert.Acknowledge(acknowledgedBy);

        // Assert
        alert.IsAcknowledged.Should().BeTrue();
        alert.AcknowledgedBy.Should().Be(acknowledgedBy);
        alert.AcknowledgedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Acknowledge_WhenAlreadyAcknowledged_ThrowsInvalidOperationException()
    {
        // Arrange
        var alert = Alert.Create(TestAlertRuleId, AlertType.Bleaching, AlertSeverity.High, "Test", "Message");
        alert.Acknowledge("first-user");

        // Act & Assert
        var act = () => alert.Acknowledge("second-user");
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Alert has already been acknowledged.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Acknowledge_WithNullOrEmptyUser_ThrowsArgumentException(string? acknowledgedBy)
    {
        // Arrange
        var alert = Alert.Create(TestAlertRuleId, AlertType.Bleaching, AlertSeverity.High, "Test", "Message");

        // Act & Assert
        var act = () => alert.Acknowledge(acknowledgedBy!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void IsExpired_WhenExpiresAtInPast_ReturnsTrue()
    {
        // Arrange
        var alert = Alert.Create(TestAlertRuleId, AlertType.Bleaching, AlertSeverity.Medium, "Test", "Message");
        alert.SetExpiration(DateTime.UtcNow.AddMilliseconds(1));

        // Wait for expiration
        Thread.Sleep(10);

        // Assert
        alert.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_WhenExpiresAtInFuture_ReturnsFalse()
    {
        // Arrange
        var alert = Alert.Create(TestAlertRuleId, AlertType.Bleaching, AlertSeverity.Medium, "Test", "Message");
        alert.SetExpiration(DateTime.UtcNow.AddHours(1));

        // Assert
        alert.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_WhenNoExpiration_ReturnsFalse()
    {
        // Arrange
        var alert = Alert.Create(TestAlertRuleId, AlertType.Bleaching, AlertSeverity.Medium, "Test", "Message");

        // Assert
        alert.ExpiresAt.Should().BeNull();
        alert.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void Escalate_IncrementsSeverityLevel()
    {
        // Arrange
        var alert = Alert.Create(TestAlertRuleId, AlertType.Bleaching, AlertSeverity.Low, "Test", "Message");

        // Act
        alert.Escalate();

        // Assert
        alert.Severity.Should().Be(AlertSeverity.Medium);
    }

    [Fact]
    public void Escalate_FromMediumToHigh()
    {
        // Arrange
        var alert = Alert.Create(TestAlertRuleId, AlertType.Bleaching, AlertSeverity.Medium, "Test", "Message");

        // Act
        alert.Escalate();

        // Assert
        alert.Severity.Should().Be(AlertSeverity.High);
    }

    [Fact]
    public void Escalate_FromHighToCritical()
    {
        // Arrange
        var alert = Alert.Create(TestAlertRuleId, AlertType.Bleaching, AlertSeverity.High, "Test", "Message");

        // Act
        alert.Escalate();

        // Assert
        alert.Severity.Should().Be(AlertSeverity.Critical);
    }

    [Fact]
    public void Escalate_WhenAlreadyCritical_ThrowsInvalidOperationException()
    {
        // Arrange
        var alert = Alert.Create(TestAlertRuleId, AlertType.Bleaching, AlertSeverity.Critical, "Test", "Message");

        // Act & Assert
        var act = () => alert.Escalate();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Alert is already at critical severity.");
    }

    [Fact]
    public void SetExpiration_WithFutureDate_SetsExpiresAt()
    {
        // Arrange
        var alert = Alert.Create(TestAlertRuleId, AlertType.Bleaching, AlertSeverity.Medium, "Test", "Message");
        var expirationTime = DateTime.UtcNow.AddDays(7);

        // Act
        alert.SetExpiration(expirationTime);

        // Assert
        alert.ExpiresAt.Should().Be(expirationTime);
    }

    [Fact]
    public void SetExpiration_WithPastDate_ThrowsArgumentException()
    {
        // Arrange
        var alert = Alert.Create(TestAlertRuleId, AlertType.Bleaching, AlertSeverity.Medium, "Test", "Message");
        var pastTime = DateTime.UtcNow.AddHours(-1);

        // Act & Assert
        var act = () => alert.SetExpiration(pastTime);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Expiration time must be in the future.*");
    }

    [Fact]
    public void Acknowledge_WhenExpired_ThrowsInvalidOperationException()
    {
        // Arrange
        var alert = Alert.Create(TestAlertRuleId, AlertType.Bleaching, AlertSeverity.High, "Test", "Message");
        alert.SetExpiration(DateTime.UtcNow.AddMilliseconds(1));
        Thread.Sleep(10);

        // Act & Assert
        var act = () => alert.Acknowledge("admin");
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot acknowledge an expired alert.");
    }

    [Fact]
    public void Escalate_WhenExpired_ThrowsInvalidOperationException()
    {
        // Arrange
        var alert = Alert.Create(TestAlertRuleId, AlertType.Bleaching, AlertSeverity.Medium, "Test", "Message");
        alert.SetExpiration(DateTime.UtcNow.AddMilliseconds(1));
        Thread.Sleep(10);

        // Act & Assert
        var act = () => alert.Escalate();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot escalate an expired alert.");
    }
}
