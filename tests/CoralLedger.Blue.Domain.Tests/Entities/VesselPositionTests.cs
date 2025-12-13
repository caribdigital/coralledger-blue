using CoralLedger.Blue.Domain.Entities;
using FluentAssertions;
using NetTopologySuite.Geometries;
using Xunit;

namespace CoralLedger.Blue.Domain.Tests.Entities;

public class VesselPositionTests
{
    private static readonly GeometryFactory GeometryFactory = new(new PrecisionModel(), 4326);

    private static Point CreateTestPoint(double lon = -77.3554, double lat = 25.0480) =>
        GeometryFactory.CreatePoint(new Coordinate(lon, lat));

    [Fact]
    public void Create_WithValidLocation_SetsProperties()
    {
        // Arrange
        var vesselId = Guid.NewGuid();
        var location = CreateTestPoint();
        var timestamp = DateTime.UtcNow;

        // Act
        var position = VesselPosition.Create(
            vesselId,
            location,
            timestamp,
            speedKnots: 8.5,
            courseOverGround: 45.0,
            heading: 48.0);

        // Assert
        position.VesselId.Should().Be(vesselId);
        position.Location.Should().Be(location);
        position.Timestamp.Should().Be(timestamp);
        position.SpeedKnots.Should().Be(8.5);
        position.CourseOverGround.Should().Be(45.0);
        position.Heading.Should().Be(48.0);
        position.Id.Should().NotBeEmpty();
        position.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithSpeed_SetsSpeed()
    {
        // Arrange
        var vesselId = Guid.NewGuid();
        var location = CreateTestPoint();
        var speedKnots = 12.3;

        // Act
        var position = VesselPosition.Create(vesselId, location, DateTime.UtcNow, speedKnots: speedKnots);

        // Assert
        position.SpeedKnots.Should().Be(speedKnots);
    }

    [Fact]
    public void Create_WithCourse_SetsCourse()
    {
        // Arrange
        var vesselId = Guid.NewGuid();
        var location = CreateTestPoint();
        var courseOverGround = 270.5; // Heading west

        // Act
        var position = VesselPosition.Create(vesselId, location, DateTime.UtcNow, courseOverGround: courseOverGround);

        // Assert
        position.CourseOverGround.Should().Be(courseOverGround);
    }

    [Fact]
    public void SetMpaContext_AssociatesWithMpa()
    {
        // Arrange
        var vesselId = Guid.NewGuid();
        var mpaId = Guid.NewGuid();
        var position = VesselPosition.Create(vesselId, CreateTestPoint(), DateTime.UtcNow);
        position.IsInMpa.Should().BeNull();
        position.MarineProtectedAreaId.Should().BeNull();

        // Act
        position.SetMpaContext(isInMpa: true, mpaId: mpaId);

        // Assert
        position.IsInMpa.Should().BeTrue();
        position.MarineProtectedAreaId.Should().Be(mpaId);
        position.ModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void SetMpaContext_OutsideMpa_SetsIsInMpaFalse()
    {
        // Arrange
        var vesselId = Guid.NewGuid();
        var position = VesselPosition.Create(vesselId, CreateTestPoint(), DateTime.UtcNow);

        // Act
        position.SetMpaContext(isInMpa: false);

        // Assert
        position.IsInMpa.Should().BeFalse();
        position.MarineProtectedAreaId.Should().BeNull();
    }

    [Fact]
    public void SetDistanceFromShore_SetsDistance()
    {
        // Arrange
        var vesselId = Guid.NewGuid();
        var position = VesselPosition.Create(vesselId, CreateTestPoint(), DateTime.UtcNow);
        var distanceKm = 15.5;

        // Act
        position.SetDistanceFromShore(distanceKm);

        // Assert
        position.DistanceFromShoreKm.Should().Be(distanceKm);
        position.ModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_SetsTimestamp()
    {
        // Arrange
        var vesselId = Guid.NewGuid();
        var timestamp = new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Utc);

        // Act
        var position = VesselPosition.Create(vesselId, CreateTestPoint(), timestamp);

        // Assert
        position.Timestamp.Should().Be(timestamp);
    }

    [Fact]
    public void Create_GeneratesUniqueId()
    {
        // Arrange
        var vesselId = Guid.NewGuid();
        var location = CreateTestPoint();

        // Act
        var position1 = VesselPosition.Create(vesselId, location, DateTime.UtcNow);
        var position2 = VesselPosition.Create(vesselId, location, DateTime.UtcNow);

        // Assert
        position1.Id.Should().NotBeEmpty();
        position2.Id.Should().NotBeEmpty();
        position1.Id.Should().NotBe(position2.Id);
    }

    [Fact]
    public void Create_LinkToVessel_SetsVesselId()
    {
        // Arrange
        var vesselId = Guid.NewGuid();

        // Act
        var position = VesselPosition.Create(vesselId, CreateTestPoint(), DateTime.UtcNow);

        // Assert
        position.VesselId.Should().Be(vesselId);
    }

    [Fact]
    public void Create_WithDifferentLocations_StoresDifferentCoordinates()
    {
        // Arrange
        var vesselId = Guid.NewGuid();
        var nassauLocation = CreateTestPoint(-77.3554, 25.0480); // Nassau
        var exumaLocation = CreateTestPoint(-76.5833, 24.4667);  // Exuma

        // Act
        var position1 = VesselPosition.Create(vesselId, nassauLocation, DateTime.UtcNow);
        var position2 = VesselPosition.Create(vesselId, exumaLocation, DateTime.UtcNow);

        // Assert
        position1.Location.X.Should().BeApproximately(-77.3554, 0.0001);
        position1.Location.Y.Should().BeApproximately(25.0480, 0.0001);
        position2.Location.X.Should().BeApproximately(-76.5833, 0.0001);
        position2.Location.Y.Should().BeApproximately(24.4667, 0.0001);
    }
}
