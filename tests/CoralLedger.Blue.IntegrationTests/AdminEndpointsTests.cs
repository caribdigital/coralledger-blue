using System.Net;
using FluentAssertions;

namespace CoralLedger.Blue.IntegrationTests;

public class AdminEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AdminEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAdminDashboard_ReturnsSuccessWithData()
    {
        // Act
        var response = await _client.GetAsync("/api/admin/dashboard");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("counts");
        content.Should().Contain("recent");
    }

    [Fact]
    public async Task GetPendingObservations_ReturnsSuccessWithPaginatedData()
    {
        // Act
        var response = await _client.GetAsync("/api/admin/observations/pending");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("items");
        content.Should().Contain("total");
        content.Should().Contain("page");
    }

    [Fact]
    public async Task GetSystemHealth_ReturnsSuccessWithStatus()
    {
        // Act
        var response = await _client.GetAsync("/api/admin/system/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("status");
        content.Should().Contain("database");
    }

    [Fact]
    public async Task GetSystemConfig_ReturnsSuccessWithFeatures()
    {
        // Act
        var response = await _client.GetAsync("/api/admin/system/config");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("features");
    }

    [Fact]
    public async Task GetDataSummary_ReturnsSuccessWithSummary()
    {
        // Act
        var response = await _client.GetAsync("/api/admin/data/summary");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("mpas");
    }

    [Fact]
    public async Task GetJobs_ReturnsSuccessWithJobList()
    {
        // Act
        var response = await _client.GetAsync("/api/admin/jobs");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("jobs");
    }
}
