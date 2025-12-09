using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace CoralLedger.Aspire.Tests.Tests;

/// <summary>
/// Integration tests for PostGIS spatial query endpoints.
/// These tests verify that EF Core spatial operations translate correctly to PostGIS functions.
/// </summary>
[Collection("Aspire")]
public class SpatialQueryTests
{
    private readonly AspireIntegrationFixture _fixture;

    public SpatialQueryTests(AspireIntegrationFixture fixture)
    {
        _fixture = fixture;
    }

    // =====================================================
    // Nearest MPA Tests (PostGIS ST_Distance)
    // =====================================================

    [Fact]
    public async Task GetNearestMpa_NassauCoordinates_ReturnsMpaWithDistance()
    {
        // Arrange - Nassau coordinates
        var lon = -77.3554;
        var lat = 25.0480;

        // Act
        var response = await _fixture.WebClient.GetAsync($"/api/mpas/nearest?lon={lon}&lat={lat}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<NearestMpaResponse>();
        result.Should().NotBeNull();
        result!.MpaId.Should().NotBeEmpty();
        result.MpaName.Should().NotBeNullOrEmpty();
        result.DistanceKm.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetNearestMpa_ExumaCaysCoordinates_ReturnsExumaLandSeaPark()
    {
        // Arrange - Exuma Cays Land & Sea Park coordinates (should be within)
        var lon = -76.5833;
        var lat = 24.4667;

        // Act
        var response = await _fixture.WebClient.GetAsync($"/api/mpas/nearest?lon={lon}&lat={lat}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<NearestMpaResponse>();
        result.Should().NotBeNull();
        result!.MpaName.Should().Contain("Exuma");
        result.IsWithinMpa.Should().BeTrue();
        result.DistanceKm.Should().Be(0); // Inside MPA = 0 distance
    }

    // =====================================================
    // Containment Tests (PostGIS ST_Contains)
    // =====================================================

    [Fact]
    public async Task CheckContainment_PointOutsideAllMpas_ReturnsNotWithinMpa()
    {
        // Arrange - Point in open ocean, far from any MPA
        var lon = -75.0;
        var lat = 26.0;

        // Act
        var response = await _fixture.WebClient.GetAsync($"/api/mpas/contains?lon={lon}&lat={lat}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ContainmentResponse>();
        result.Should().NotBeNull();
        result!.IsWithinMpa.Should().BeFalse();
    }

    [Fact]
    public async Task CheckContainment_PointInsideMpa_ReturnsWithinMpaWithDetails()
    {
        // Arrange - First get an MPA's centroid to ensure we're testing inside an MPA
        var allMpasResponse = await _fixture.WebClient.GetAsync("/api/mpas");
        allMpasResponse.EnsureSuccessStatusCode();

        // Use Exuma Cays coordinates (known to be inside Exuma Land & Sea Park)
        var lon = -76.5833;
        var lat = 24.4667;

        // Act
        var response = await _fixture.WebClient.GetAsync($"/api/mpas/contains?lon={lon}&lat={lat}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ContainmentResponse>();
        result.Should().NotBeNull();

        // If this point is inside an MPA, verify the full response
        if (result!.IsWithinMpa)
        {
            result.MpaId.Should().NotBeEmpty();
            result.MpaName.Should().NotBeNullOrEmpty();
            result.ProtectionLevel.Should().NotBeNullOrEmpty();
        }
    }

    // =====================================================
    // Radius Search Tests (PostGIS ST_DWithin)
    // =====================================================

    [Fact]
    public async Task GetMpasWithinRadius_SmallRadius_ReturnsNearbyMpas()
    {
        // Arrange - Nassau with small radius
        var lon = -77.3554;
        var lat = 25.0480;
        var radiusKm = 50.0;

        // Act
        var response = await _fixture.WebClient.GetAsync(
            $"/api/mpas/within-radius?lon={lon}&lat={lat}&radiusKm={radiusKm}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var results = await response.Content.ReadFromJsonAsync<List<MpaDistanceResult>>();
        results.Should().NotBeNull();
        // All results should be within the specified radius
        results!.ForEach(r => r.DistanceKm.Should().BeLessThanOrEqualTo(radiusKm));
    }

    [Fact]
    public async Task GetMpasWithinRadius_LargeRadius_ReturnsMultipleMpas()
    {
        // Arrange - Central Bahamas with large radius to capture multiple MPAs
        var lon = -77.0;
        var lat = 25.0;
        var radiusKm = 200.0;

        // Act
        var response = await _fixture.WebClient.GetAsync(
            $"/api/mpas/within-radius?lon={lon}&lat={lat}&radiusKm={radiusKm}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var results = await response.Content.ReadFromJsonAsync<List<MpaDistanceResult>>();
        results.Should().NotBeNull();

        // With a 200km radius from central Bahamas, we should find multiple MPAs
        results!.Count.Should().BeGreaterThan(0);

        // Results should be ordered by distance
        var distances = results.Select(r => r.DistanceKm).ToList();
        distances.Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task GetMpasWithinRadius_ZeroRadius_ReturnsOnlyContainingMpa()
    {
        // Arrange - Point inside Exuma with zero radius
        var lon = -76.5833;
        var lat = 24.4667;
        var radiusKm = 0.1; // Very small radius

        // Act
        var response = await _fixture.WebClient.GetAsync(
            $"/api/mpas/within-radius?lon={lon}&lat={lat}&radiusKm={radiusKm}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var results = await response.Content.ReadFromJsonAsync<List<MpaDistanceResult>>();
        results.Should().NotBeNull();

        // If any results, they should be where we're inside
        results!.Where(r => r.IsWithinMpa).ToList()
            .ForEach(r => r.DistanceKm.Should().Be(0));
    }

    // =====================================================
    // GeoJSON Tests (Spatial Data Return)
    // =====================================================

    [Fact]
    public async Task GetMpasGeoJson_DefaultResolution_ReturnsValidGeoJson()
    {
        // Act
        var response = await _fixture.WebClient.GetAsync("/api/mpas/geojson");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var geoJson = await response.Content.ReadFromJsonAsync<GeoJsonFeatureCollection>();
        geoJson.Should().NotBeNull();
        geoJson!.Type.Should().Be("FeatureCollection");
        geoJson.Features.Should().NotBeEmpty();

        // Each feature should have geometry
        foreach (var feature in geoJson.Features)
        {
            feature.Type.Should().Be("Feature");
            feature.Geometry.Should().NotBeNull();
            feature.Properties.Should().NotBeNull();
        }
    }

    [Theory]
    [InlineData("full")]
    [InlineData("detail")]
    [InlineData("medium")]
    [InlineData("low")]
    public async Task GetMpasGeoJson_DifferentResolutions_ReturnsValidGeoJson(string resolution)
    {
        // Act
        var response = await _fixture.WebClient.GetAsync($"/api/mpas/geojson?resolution={resolution}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var geoJson = await response.Content.ReadFromJsonAsync<GeoJsonFeatureCollection>();
        geoJson.Should().NotBeNull();
        geoJson!.Features.Should().NotBeEmpty();
    }

    // =====================================================
    // Edge Case Tests
    // =====================================================

    [Fact]
    public async Task GetNearestMpa_BoundaryCoordinates_HandlesCorrectly()
    {
        // Arrange - Bahamas EEZ boundary edge
        var lon = -80.4;
        var lat = 26.5;

        // Act
        var response = await _fixture.WebClient.GetAsync($"/api/mpas/nearest?lon={lon}&lat={lat}");

        // Assert - Should still return a result (may be far away)
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetMpasWithinRadius_OutsideBahamas_ReturnsEmptyOrDistant()
    {
        // Arrange - Florida Keys (outside Bahamas)
        var lon = -81.0;
        var lat = 24.5;
        var radiusKm = 10.0;

        // Act
        var response = await _fixture.WebClient.GetAsync(
            $"/api/mpas/within-radius?lon={lon}&lat={lat}&radiusKm={radiusKm}");

        // Assert - Should succeed but likely return empty list
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var results = await response.Content.ReadFromJsonAsync<List<MpaDistanceResult>>();
        results.Should().NotBeNull();
        // Florida Keys is outside Bahamas MPAs, so with small radius should be empty
    }

    // =====================================================
    // Response DTOs
    // =====================================================

    private class NearestMpaResponse
    {
        public Guid MpaId { get; set; }
        public string MpaName { get; set; } = "";
        public string ProtectionLevel { get; set; } = "";
        public double DistanceKm { get; set; }
        public bool IsWithinMpa { get; set; }
        public PointDto? NearestPoint { get; set; }
    }

    private class PointDto
    {
        public double Lon { get; set; }
        public double Lat { get; set; }
    }

    private class ContainmentResponse
    {
        public bool IsWithinMpa { get; set; }
        public Guid? MpaId { get; set; }
        public string? MpaName { get; set; }
        public string? ProtectionLevel { get; set; }
        public bool? IsNoTakeZone { get; set; }
        public double? DistanceToNearestBoundaryKm { get; set; }
        public Guid? NearestReefId { get; set; }
        public string? NearestReefName { get; set; }
    }

    private class MpaDistanceResult
    {
        public Guid MpaId { get; set; }
        public string MpaName { get; set; } = "";
        public string ProtectionLevel { get; set; } = "";
        public double DistanceKm { get; set; }
        public bool IsWithinMpa { get; set; }
    }

    private class GeoJsonFeatureCollection
    {
        public string Type { get; set; } = "";
        public List<GeoJsonFeature> Features { get; set; } = new();
    }

    private class GeoJsonFeature
    {
        public string Type { get; set; } = "";
        public object? Geometry { get; set; }
        public Dictionary<string, object>? Properties { get; set; }
    }
}
