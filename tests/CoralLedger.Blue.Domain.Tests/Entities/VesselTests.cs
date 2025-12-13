using CoralLedger.Blue.Domain.Entities;
using CoralLedger.Blue.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace CoralLedger.Blue.Domain.Tests.Entities;

public class VesselTests
{
    [Fact]
    public void Create_WithValidData_SetsAllProperties()
    {
        // Arrange
        var name = "F/V Blue Marlin";
        var mmsi = "123456789";
        var imo = "1234567";
        var gfwVesselId = "gfw-12345";
        var flag = "BHS";

        // Act
        var vessel = Vessel.Create(
            name,
            mmsi: mmsi,
            imo: imo,
            gfwVesselId: gfwVesselId,
            flag: flag,
            vesselType: VesselType.Fishing,
            gearType: GearType.Longliners);

        // Assert
        vessel.Name.Should().Be(name);
        vessel.Mmsi.Should().Be(mmsi);
        vessel.Imo.Should().Be(imo);
        vessel.GfwVesselId.Should().Be(gfwVesselId);
        vessel.Flag.Should().Be(flag);
        vessel.VesselType.Should().Be(VesselType.Fishing);
        vessel.GearType.Should().Be(GearType.Longliners);
        vessel.IsActive.Should().BeTrue();
        vessel.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_GeneratesUniqueId()
    {
        // Act
        var vessel1 = Vessel.Create("Vessel 1");
        var vessel2 = Vessel.Create("Vessel 2");

        // Assert
        vessel1.Id.Should().NotBeEmpty();
        vessel2.Id.Should().NotBeEmpty();
        vessel1.Id.Should().NotBe(vessel2.Id);
    }

    [Fact]
    public void Create_WithMmsi_SetsMmsi()
    {
        // Arrange
        var mmsi = "311000123";

        // Act
        var vessel = Vessel.Create("Test Vessel", mmsi: mmsi);

        // Assert
        vessel.Mmsi.Should().Be(mmsi);
    }

    [Fact]
    public void Create_WithImo_SetsImo()
    {
        // Arrange
        var imo = "9876543";

        // Act
        var vessel = Vessel.Create("Test Vessel", imo: imo);

        // Assert
        vessel.Imo.Should().Be(imo);
    }

    [Theory]
    [InlineData(VesselType.Unknown)]
    [InlineData(VesselType.Fishing)]
    [InlineData(VesselType.Carrier)]
    [InlineData(VesselType.Support)]
    [InlineData(VesselType.Cargo)]
    [InlineData(VesselType.Tanker)]
    [InlineData(VesselType.Passenger)]
    [InlineData(VesselType.Recreational)]
    [InlineData(VesselType.Research)]
    [InlineData(VesselType.Other)]
    public void Create_SupportsAllVesselTypes(VesselType vesselType)
    {
        // Act
        var vessel = Vessel.Create("Test Vessel", vesselType: vesselType);

        // Assert
        vessel.VesselType.Should().Be(vesselType);
    }

    [Theory]
    [InlineData(GearType.Unknown)]
    [InlineData(GearType.Trawlers)]
    [InlineData(GearType.PurseSeiners)]
    [InlineData(GearType.Longliners)]
    [InlineData(GearType.FixedGear)]
    [InlineData(GearType.Dredge)]
    [InlineData(GearType.PolesAndLines)]
    [InlineData(GearType.Trollers)]
    [InlineData(GearType.Gillnets)]
    [InlineData(GearType.Squid)]
    [InlineData(GearType.Other)]
    public void Create_SupportsAllGearTypes(GearType gearType)
    {
        // Act
        var vessel = Vessel.Create("Test Vessel", gearType: gearType);

        // Assert
        vessel.GearType.Should().Be(gearType);
    }

    [Fact]
    public void UpdateFromApi_UpdatesAllFields()
    {
        // Arrange
        var vessel = Vessel.Create("Original Name", vesselType: VesselType.Unknown);
        var originalModifiedAt = vessel.ModifiedAt;

        // Act
        vessel.UpdateFromApi(
            name: "Updated Name",
            callSign: "ABC123",
            flag: "BHS",
            vesselType: VesselType.Fishing,
            gearType: GearType.Longliners,
            lengthMeters: 25.5,
            tonnageGt: 150.0,
            yearBuilt: 2015);

        // Assert
        vessel.Name.Should().Be("Updated Name");
        vessel.CallSign.Should().Be("ABC123");
        vessel.Flag.Should().Be("BHS");
        vessel.VesselType.Should().Be(VesselType.Fishing);
        vessel.GearType.Should().Be(GearType.Longliners);
        vessel.LengthMeters.Should().Be(25.5);
        vessel.TonnageGt.Should().Be(150.0);
        vessel.YearBuilt.Should().Be(2015);
        vessel.ModifiedAt.Should().NotBe(originalModifiedAt);
        vessel.ModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void UpdateLastPosition_SetsPositionTimeAndModifiedAt()
    {
        // Arrange
        var vessel = Vessel.Create("Test Vessel");
        var positionTime = DateTime.UtcNow.AddMinutes(-5);

        // Act
        vessel.UpdateLastPosition(positionTime);

        // Assert
        vessel.LastPositionTime.Should().Be(positionTime);
        vessel.ModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Deactivate_SetsStatusToInactive()
    {
        // Arrange
        var vessel = Vessel.Create("Test Vessel");
        vessel.IsActive.Should().BeTrue();

        // Act
        vessel.Deactivate();

        // Assert
        vessel.IsActive.Should().BeFalse();
        vessel.ModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithFlag_SetsFlag()
    {
        // Arrange
        var flag = "BHS"; // Bahamas

        // Act
        var vessel = Vessel.Create("Test Vessel", flag: flag);

        // Assert
        vessel.Flag.Should().Be(flag);
    }

    [Fact]
    public void Create_WithoutOptionalParameters_SetsDefaults()
    {
        // Act
        var vessel = Vessel.Create("Minimal Vessel");

        // Assert
        vessel.Name.Should().Be("Minimal Vessel");
        vessel.Mmsi.Should().BeNull();
        vessel.Imo.Should().BeNull();
        vessel.CallSign.Should().BeNull();
        vessel.GfwVesselId.Should().BeNull();
        vessel.Flag.Should().BeNull();
        vessel.VesselType.Should().Be(VesselType.Unknown);
        vessel.GearType.Should().BeNull();
        vessel.LengthMeters.Should().BeNull();
        vessel.TonnageGt.Should().BeNull();
        vessel.YearBuilt.Should().BeNull();
        vessel.LastPositionTime.Should().BeNull();
        vessel.IsActive.Should().BeTrue();
    }
}
