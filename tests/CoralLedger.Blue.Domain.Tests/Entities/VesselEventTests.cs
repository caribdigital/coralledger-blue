using CoralLedger.Blue.Domain.Entities;
using CoralLedger.Blue.Domain.Enums;
using FluentAssertions;
using NetTopologySuite.Geometries;
using Xunit;

namespace CoralLedger.Blue.Domain.Tests.Entities;

public class VesselEventTests
{
    private static readonly GeometryFactory GeometryFactory = new(new PrecisionModel(), 4326);

    private static Point CreateTestPoint(double lon = -77.3554, double lat = 25.0480) =>
        GeometryFactory.CreatePoint(new Coordinate(lon, lat));

    [Fact]
    public void CreateFishingEvent_SetsEventTypeToFishing()
    {
        // Arrange
        var vesselId = Guid.NewGuid();
        var location = CreateTestPoint();
        var startTime = DateTime.UtcNow.AddHours(-2);
        var endTime = DateTime.UtcNow;

        // Act
        var fishingEvent = VesselEvent.CreateFishingEvent(
            vesselId,
            location,
            startTime,
            endTime,
            durationHours: 2.0,
            distanceKm: 5.5,
            gfwEventId: "gfw-fishing-123");

        // Assert
        fishingEvent.EventType.Should().Be(VesselEventType.Fishing);
        fishingEvent.VesselId.Should().Be(vesselId);
        fishingEvent.Location.Should().Be(location);
        fishingEvent.StartTime.Should().Be(startTime);
        fishingEvent.EndTime.Should().Be(endTime);
        fishingEvent.DurationHours.Should().Be(2.0);
        fishingEvent.DistanceKm.Should().Be(5.5);
        fishingEvent.GfwEventId.Should().Be("gfw-fishing-123");
    }

    [Fact]
    public void CreatePortVisit_SetsEventTypeToPortVisit()
    {
        // Arrange
        var vesselId = Guid.NewGuid();
        var location = CreateTestPoint();
        var startTime = DateTime.UtcNow.AddDays(-1);
        var endTime = DateTime.UtcNow;
        var portName = "Nassau Harbor";

        // Act
        var portVisit = VesselEvent.CreatePortVisit(
            vesselId,
            location,
            startTime,
            endTime,
            portName,
            gfwEventId: "gfw-port-456");

        // Assert
        portVisit.EventType.Should().Be(VesselEventType.PortVisit);
        portVisit.PortName.Should().Be(portName);
        portVisit.VesselId.Should().Be(vesselId);
        portVisit.StartTime.Should().Be(startTime);
        portVisit.EndTime.Should().Be(endTime);
        portVisit.GfwEventId.Should().Be("gfw-port-456");
    }

    [Fact]
    public void CreateEncounter_SetsEventTypeToEncounter()
    {
        // Arrange
        var vesselId = Guid.NewGuid();
        var location = CreateTestPoint();
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
        var encounterVesselId = "other-vessel-789";

        // Act
        var encounter = VesselEvent.CreateEncounter(
            vesselId,
            location,
            startTime,
            endTime,
            encounterVesselId,
            gfwEventId: "gfw-encounter-789");

        // Assert
        encounter.EventType.Should().Be(VesselEventType.Encounter);
        encounter.EncounterVesselId.Should().Be(encounterVesselId);
        encounter.VesselId.Should().Be(vesselId);
        encounter.StartTime.Should().Be(startTime);
        encounter.EndTime.Should().Be(endTime);
    }

    [Fact]
    public void Create_WithDuration_SetsDuration()
    {
        // Arrange
        var vesselId = Guid.NewGuid();
        var location = CreateTestPoint();
        var durationHours = 4.5;

        // Act
        var fishingEvent = VesselEvent.CreateFishingEvent(
            vesselId,
            location,
            DateTime.UtcNow,
            null,
            durationHours: durationHours,
            distanceKm: null);

        // Assert
        fishingEvent.DurationHours.Should().Be(durationHours);
    }

    [Fact]
    public void Create_WithLocation_SetsLocation()
    {
        // Arrange
        var vesselId = Guid.NewGuid();
        var exumaLocation = CreateTestPoint(-76.5833, 24.4667);

        // Act
        var fishingEvent = VesselEvent.CreateFishingEvent(
            vesselId,
            exumaLocation,
            DateTime.UtcNow,
            null,
            durationHours: null,
            distanceKm: null);

        // Assert
        fishingEvent.Location.Should().Be(exumaLocation);
        fishingEvent.Location.X.Should().BeApproximately(-76.5833, 0.0001);
        fishingEvent.Location.Y.Should().BeApproximately(24.4667, 0.0001);
    }

    [Fact]
    public void SetMpaContext_AssociatesWithMpa()
    {
        // Arrange
        var vesselId = Guid.NewGuid();
        var mpaId = Guid.NewGuid();
        var fishingEvent = VesselEvent.CreateFishingEvent(
            vesselId,
            CreateTestPoint(),
            DateTime.UtcNow,
            null,
            durationHours: null,
            distanceKm: null);
        fishingEvent.IsInMpa.Should().BeNull();

        // Act
        fishingEvent.SetMpaContext(isInMpa: true, mpaId: mpaId);

        // Assert
        fishingEvent.IsInMpa.Should().BeTrue();
        fishingEvent.MarineProtectedAreaId.Should().Be(mpaId);
        fishingEvent.ModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithStartTime_SetsStartTime()
    {
        // Arrange
        var vesselId = Guid.NewGuid();
        var startTime = new DateTime(2024, 6, 15, 8, 0, 0, DateTimeKind.Utc);

        // Act
        var fishingEvent = VesselEvent.CreateFishingEvent(
            vesselId,
            CreateTestPoint(),
            startTime,
            null,
            durationHours: null,
            distanceKm: null);

        // Assert
        fishingEvent.StartTime.Should().Be(startTime);
    }

    [Fact]
    public void Create_WithEndTime_SetsEndTime()
    {
        // Arrange
        var vesselId = Guid.NewGuid();
        var startTime = new DateTime(2024, 6, 15, 8, 0, 0, DateTimeKind.Utc);
        var endTime = new DateTime(2024, 6, 15, 12, 0, 0, DateTimeKind.Utc);

        // Act
        var fishingEvent = VesselEvent.CreateFishingEvent(
            vesselId,
            CreateTestPoint(),
            startTime,
            endTime,
            durationHours: 4.0,
            distanceKm: null);

        // Assert
        fishingEvent.EndTime.Should().Be(endTime);
    }

    [Fact]
    public void Create_GeneratesUniqueId()
    {
        // Arrange
        var vesselId = Guid.NewGuid();

        // Act
        var event1 = VesselEvent.CreateFishingEvent(vesselId, CreateTestPoint(), DateTime.UtcNow, null, null, null);
        var event2 = VesselEvent.CreateFishingEvent(vesselId, CreateTestPoint(), DateTime.UtcNow, null, null, null);

        // Assert
        event1.Id.Should().NotBeEmpty();
        event2.Id.Should().NotBeEmpty();
        event1.Id.Should().NotBe(event2.Id);
    }

    [Fact]
    public void Create_LinkToVessel_SetsVesselId()
    {
        // Arrange
        var vesselId = Guid.NewGuid();

        // Act
        var fishingEvent = VesselEvent.CreateFishingEvent(
            vesselId,
            CreateTestPoint(),
            DateTime.UtcNow,
            null,
            durationHours: null,
            distanceKm: null);

        // Assert
        fishingEvent.VesselId.Should().Be(vesselId);
    }

    [Fact]
    public void CreateFishingEvent_WithDistanceKm_SetsDistance()
    {
        // Arrange
        var vesselId = Guid.NewGuid();
        var distanceKm = 12.5;

        // Act
        var fishingEvent = VesselEvent.CreateFishingEvent(
            vesselId,
            CreateTestPoint(),
            DateTime.UtcNow,
            null,
            durationHours: null,
            distanceKm: distanceKm);

        // Assert
        fishingEvent.DistanceKm.Should().Be(distanceKm);
    }
}
