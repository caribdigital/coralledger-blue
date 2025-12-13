using System.Net;
using FluentAssertions;

namespace CoralLedger.Blue.IntegrationTests;

public class AisEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AisEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAisStatus_ReturnsSuccessWithStatus()
    {
        // Act
        var response = await _client.GetAsync("/api/ais/status");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("configured");
        content.Should().Contain("message");
    }

    [Fact]
    public async Task GetAisVessels_ReturnsSuccessWithVesselData()
    {
        // Act
        var response = await _client.GetAsync("/api/ais/vessels");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("count");
        content.Should().Contain("vessels");
    }

    [Fact]
    public async Task GetAisVesselsNear_ReturnsSuccessWithFilteredData()
    {
        // Arrange - coordinates near Nassau
        var lon = -77.35;
        var lat = 25.05;
        var radiusKm = 50;

        // Act
        var response = await _client.GetAsync($"/api/ais/vessels/near?lon={lon}&lat={lat}&radiusKm={radiusKm}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("center");
        content.Should().Contain("vessels");
    }

    [Fact]
    public async Task GetAisVesselTrack_ReturnsSuccessWithTrackData()
    {
        // Arrange
        var mmsi = "311000001"; // Demo vessel MMSI
        var hours = 24;

        // Act
        var response = await _client.GetAsync($"/api/ais/vessels/{mmsi}/track?hours={hours}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("mmsi");
        content.Should().Contain("track");
    }
}
