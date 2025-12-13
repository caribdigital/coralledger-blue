using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace CoralLedger.Blue.IntegrationTests;

public class MpaEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public MpaEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetMpas_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/api/mpas");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetMpas_ReturnsJsonContent()
    {
        // Act
        var response = await _client.GetAsync("/api/mpas");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetMpas_WithIslandGroupFilter_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/mpas?islandGroup=Exumas");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetMpaById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/mpas/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetMpaStats_ReturnsSuccessWithData()
    {
        // Act
        var response = await _client.GetAsync("/api/mpas/stats");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("totalCount");
    }
}
