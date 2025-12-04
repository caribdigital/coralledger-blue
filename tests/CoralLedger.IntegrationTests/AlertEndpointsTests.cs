using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace CoralLedger.IntegrationTests;

public class AlertEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AlertEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAlerts_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/api/alerts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAlerts_WithTypeFilter_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/alerts?type=Bleaching");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAlertRules_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/api/alerts/rules");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAlertStats_ReturnsSuccessWithData()
    {
        // Act
        var response = await _client.GetAsync("/api/alerts/stats");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("period");
    }

    [Fact]
    public async Task GetAlert_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/alerts/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateAlertRule_WithValidData_ReturnsCreated()
    {
        // Arrange
        var request = new
        {
            name = "Test Bleaching Rule",
            type = "Bleaching",
            severity = "Medium",
            conditions = "{\"minAlertLevel\": 2}"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/alerts/rules", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateAlertRule_WithInvalidType_ReturnsBadRequest()
    {
        // Arrange
        var request = new
        {
            name = "Invalid Rule",
            type = "InvalidType",
            conditions = "{}"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/alerts/rules", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
