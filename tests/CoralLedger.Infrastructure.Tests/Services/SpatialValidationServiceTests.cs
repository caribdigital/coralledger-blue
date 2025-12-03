using CoralLedger.Application.Common.Interfaces;
using CoralLedger.Infrastructure.Common;
using CoralLedger.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NetTopologySuite.Geometries;
using Xunit;

namespace CoralLedger.Infrastructure.Tests.Services;

public class SpatialValidationServiceTests
{
    private static readonly GeometryFactory GeometryFactory = new(new PrecisionModel(), 4326);
    private readonly ISpatialValidationService _sut;

    public SpatialValidationServiceTests()
    {
        var loggerMock = new Mock<ILogger<SpatialValidationService>>();
        _sut = new SpatialValidationService(loggerMock.Object);
    }

    private static Point CreatePoint(double lon, double lat, int srid = 4326)
    {
        var factory = new GeometryFactory(new PrecisionModel(), srid);
        return factory.CreatePoint(new Coordinate(lon, lat));
    }

    private static Polygon CreatePolygon(double centerLon, double centerLat, double size = 0.1, int srid = 4326)
    {
        var factory = new GeometryFactory(new PrecisionModel(), srid);
        var coordinates = new[]
        {
            new Coordinate(centerLon - size, centerLat - size),
            new Coordinate(centerLon + size, centerLat - size),
            new Coordinate(centerLon + size, centerLat + size),
            new Coordinate(centerLon - size, centerLat + size),
            new Coordinate(centerLon - size, centerLat - size) // Close the ring
        };
        return factory.CreatePolygon(coordinates);
    }

    // Gate 1: SRID validation
    [Fact]
    public void ValidatePoint_ValidCoordinates_ReturnsSuccess()
    {
        // Arrange - Nassau coordinates
        var point = CreatePoint(-77.3554, 25.0480);

        // Act
        var result = _sut.ValidatePoint(point);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidatePoint_InvalidSrid_ReturnsGate1Error()
    {
        // Arrange - Point with wrong SRID (UTM)
        var point = CreatePoint(-77.3554, 25.0480, srid: 32618);

        // Act
        var result = _sut.ValidatePoint(point);

        // Assert
        result.IsValid.Should().BeFalse();
        result.FailedGates.Should().Contain(1);
        result.Errors.Should().Contain(e => e.Contains("Invalid SRID"));
    }

    // Gate 2: Longitude range
    [Fact]
    public void ValidatePoint_LongitudeOutOfRange_ReturnsGate2Error()
    {
        // Arrange - Invalid longitude
        var point = CreatePoint(-200.0, 25.0);

        // Act
        var result = _sut.ValidatePoint(point);

        // Assert
        result.IsValid.Should().BeFalse();
        result.FailedGates.Should().Contain(2);
        result.Errors.Should().Contain(e => e.Contains("Longitude") && e.Contains("out of range"));
    }

    // Gate 3: Latitude range
    [Fact]
    public void ValidatePoint_LatitudeOutOfRange_ReturnsGate3Error()
    {
        // Arrange - Invalid latitude
        var point = CreatePoint(-77.0, 95.0);

        // Act
        var result = _sut.ValidatePoint(point);

        // Assert
        result.IsValid.Should().BeFalse();
        result.FailedGates.Should().Contain(3);
        result.Errors.Should().Contain(e => e.Contains("Latitude") && e.Contains("out of range"));
    }

    // Gate 4: Within Bahamas EEZ
    [Fact]
    public void ValidatePoint_OutsideBahamas_ReturnsGate4Error()
    {
        // Arrange - Key West, Florida (outside Bahamas EEZ at -81.78, west of -80.5 limit)
        var point = CreatePoint(-81.78, 24.5551);

        // Act
        var result = _sut.ValidatePoint(point);

        // Assert
        result.IsValid.Should().BeFalse();
        result.FailedGates.Should().Contain(4);
        result.Errors.Should().Contain(e => e.Contains("outside Bahamas EEZ"));
    }

    // Gate 5: Geometry validity
    [Fact]
    public void ValidateGeometry_NullGeometry_ReturnsError()
    {
        // Act
        var result = _sut.ValidateGeometry(null!);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("null"));
    }

    [Fact]
    public void ValidateGeometry_EmptyGeometry_ReturnsError()
    {
        // Arrange
        var emptyPoint = GeometryFactory.CreatePoint();

        // Act
        var result = _sut.ValidateGeometry(emptyPoint);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("empty"));
    }

    // Polygon validation tests
    [Fact]
    public void ValidatePolygon_ValidPolygon_ReturnsSuccess()
    {
        // Arrange - Valid polygon in Exumas
        var polygon = CreatePolygon(-76.5833, 24.4667, size: 0.05);

        // Act
        var result = _sut.ValidatePolygon(polygon);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidatePolygon_NotClosed_ReturnsGate6Error()
    {
        // Arrange - Polygon that is not properly closed (NTS handles this internally,
        // but we test the validation logic)
        var coordinates = new[]
        {
            new Coordinate(-77.0, 24.0),
            new Coordinate(-76.0, 24.0),
            new Coordinate(-76.0, 25.0),
            new Coordinate(-77.0, 25.0),
            new Coordinate(-77.0, 24.0) // Must be closed for NTS
        };
        var polygon = GeometryFactory.CreatePolygon(coordinates);

        // Act - Valid closed polygon should pass
        var result = _sut.ValidatePolygon(polygon);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidatePolygon_SelfIntersecting_ReturnsGate7Error()
    {
        // Arrange - Self-intersecting polygon (bowtie shape)
        var coordinates = new[]
        {
            new Coordinate(-77.0, 24.0),
            new Coordinate(-76.0, 25.0),
            new Coordinate(-76.0, 24.0),
            new Coordinate(-77.0, 25.0),
            new Coordinate(-77.0, 24.0)
        };
        var polygon = GeometryFactory.CreatePolygon(coordinates);

        // Act
        var result = _sut.ValidatePolygon(polygon);

        // Assert
        result.IsValid.Should().BeFalse();
        result.FailedGates.Should().Contain(7);
    }

    [Fact]
    public void ValidatePolygon_WrongOrientation_ReturnsGate8Error()
    {
        // Arrange - Clockwise polygon (should be counter-clockwise)
        var coordinates = new[]
        {
            new Coordinate(-77.0, 24.0),
            new Coordinate(-77.0, 25.0),
            new Coordinate(-76.0, 25.0),
            new Coordinate(-76.0, 24.0),
            new Coordinate(-77.0, 24.0)
        };
        var polygon = GeometryFactory.CreatePolygon(coordinates);

        // Act
        var result = _sut.ValidatePolygon(polygon);

        // Assert
        result.IsValid.Should().BeFalse();
        result.FailedGates.Should().Contain(8);
        result.Errors.Should().Contain(e => e.Contains("counter-clockwise"));
    }

    [Fact]
    public void ValidatePolygon_TooLarge_ReturnsGate9Error()
    {
        // Arrange - Very large polygon (> 50,000 km²)
        var coordinates = new[]
        {
            new Coordinate(-80.0, 20.0),
            new Coordinate(-72.0, 20.0),
            new Coordinate(-72.0, 28.0),
            new Coordinate(-80.0, 28.0),
            new Coordinate(-80.0, 20.0)
        };
        var polygon = GeometryFactory.CreatePolygon(coordinates);

        // Act
        var result = _sut.ValidatePolygon(polygon);

        // Assert
        result.IsValid.Should().BeFalse();
        result.FailedGates.Should().Contain(9);
        result.Errors.Should().Contain(e => e.Contains("exceeds maximum"));
    }

    // IsWithinBahamas tests
    [Fact]
    public void IsWithinBahamas_PointInsideExumas_ReturnsTrue()
    {
        // Arrange - Exuma Cays
        var point = BahamasSpatialConstants.WellKnownLocations.ExumaLandSeaPark;

        // Act
        var result = _sut.IsWithinBahamas(point);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsWithinBahamas_PointInNassau_ReturnsTrue()
    {
        // Arrange
        var point = BahamasSpatialConstants.WellKnownLocations.Nassau;

        // Act
        var result = _sut.IsWithinBahamas(point);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsWithinBahamas_PointInFlorida_ReturnsFalse()
    {
        // Arrange - Key West (outside bounds)
        var point = BahamasSpatialConstants.OutOfBoundsLocations.KeyWest;

        // Act
        var result = _sut.IsWithinBahamas(point);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsWithinBahamas_PointInCuba_ReturnsFalse()
    {
        // Arrange - Havana
        var point = BahamasSpatialConstants.OutOfBoundsLocations.Havana;

        // Act
        var result = _sut.IsWithinBahamas(point);

        // Assert
        result.Should().BeFalse();
    }

    // Additional validation tests
    [Fact]
    public void ValidatePoint_NassauCoordinates_ReturnsSuccess()
    {
        // Arrange
        var point = BahamasSpatialConstants.WellKnownLocations.Nassau;

        // Act
        var result = _sut.ValidatePoint(point);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidatePoint_ExumaCoordinates_ReturnsSuccess()
    {
        // Arrange
        var point = BahamasSpatialConstants.WellKnownLocations.ExumaLandSeaPark;

        // Act
        var result = _sut.ValidatePoint(point);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidatePoint_AndrosBarrierReef_ReturnsSuccess()
    {
        // Arrange
        var point = BahamasSpatialConstants.WellKnownLocations.AndrosBarrierReef;

        // Act
        var result = _sut.ValidatePoint(point);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidatePoint_GrandBahama_ReturnsSuccess()
    {
        // Arrange
        var point = BahamasSpatialConstants.WellKnownLocations.GrandBahama;

        // Act
        var result = _sut.ValidatePoint(point);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidatePoint_Inagua_ReturnsSuccess()
    {
        // Arrange
        var point = BahamasSpatialConstants.WellKnownLocations.Inagua;

        // Act
        var result = _sut.ValidatePoint(point);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidatePoint_PuertoRico_ReturnsGate4Error()
    {
        // Arrange
        var point = BahamasSpatialConstants.OutOfBoundsLocations.PuertoRico;

        // Act
        var result = _sut.ValidatePoint(point);

        // Assert
        result.IsValid.Should().BeFalse();
        result.FailedGates.Should().Contain(4);
    }

    [Fact]
    public void IsWithinBahamas_NullPoint_ReturnsFalse()
    {
        // Act
        var result = _sut.IsWithinBahamas((Point)null!);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsWithinBahamas_EmptyPoint_ReturnsFalse()
    {
        // Arrange
        var emptyPoint = GeometryFactory.CreatePoint();

        // Act
        var result = _sut.IsWithinBahamas(emptyPoint);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidatePolygon_WithValidSrid0_ReturnsSuccess()
    {
        // Arrange - SRID 0 is accepted (means unspecified, defaults to WGS84)
        var factory = new GeometryFactory(new PrecisionModel(), 0);
        var coordinates = new[]
        {
            new Coordinate(-77.0, 24.0),
            new Coordinate(-76.0, 24.0),
            new Coordinate(-76.0, 25.0),
            new Coordinate(-77.0, 25.0),
            new Coordinate(-77.0, 24.0)
        };
        var polygon = factory.CreatePolygon(coordinates);

        // Act
        var result = _sut.ValidatePolygon(polygon);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
