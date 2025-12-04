using System.Net;
using FluentAssertions;

namespace CoralLedger.IntegrationTests;

public class ExportEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ExportEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ExportMpasGeoJson_ReturnsGeoJsonContent()
    {
        // Act
        var response = await _client.GetAsync("/api/export/mpas/geojson");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/geo+json");

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("FeatureCollection");
    }

    [Fact]
    public async Task ExportMpasCsv_ReturnsCsvContent()
    {
        // Act
        var response = await _client.GetAsync("/api/export/mpas/csv");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/csv");

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Id,Name,IslandGroup");
    }

    [Fact]
    public async Task ExportMpasShapefile_ReturnsZipFile()
    {
        // Act
        var response = await _client.GetAsync("/api/export/mpas/shapefile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/zip");
    }

    [Fact]
    public async Task ExportVesselsGeoJson_ReturnsGeoJsonContent()
    {
        // Act
        var response = await _client.GetAsync("/api/export/vessels/geojson");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("FeatureCollection");
    }

    [Fact]
    public async Task ExportBleachingGeoJson_ReturnsGeoJsonContent()
    {
        // Act
        var response = await _client.GetAsync("/api/export/bleaching/geojson");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("FeatureCollection");
    }

    [Fact]
    public async Task ExportObservationsGeoJson_ReturnsGeoJsonContent()
    {
        // Act
        var response = await _client.GetAsync("/api/export/observations/geojson");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("FeatureCollection");
    }

    [Fact]
    public async Task ExportWithDateRange_ReturnsFilteredData()
    {
        // Arrange
        var fromDate = DateTime.UtcNow.AddDays(-7).ToString("o");
        var toDate = DateTime.UtcNow.ToString("o");

        // Act
        var response = await _client.GetAsync($"/api/export/vessels/geojson?fromDate={fromDate}&toDate={toDate}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
