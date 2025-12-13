using System.Net;
using System.Text.Json;

namespace CoralLedger.Blue.Aspire.Tests.Tests;

/// <summary>
/// Tests for web application health and readiness endpoints
/// </summary>
[Collection("Aspire")]
public class WebAppHealthTests
{
    private readonly AspireIntegrationFixture _fixture;

    public WebAppHealthTests(AspireIntegrationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task DiagnosticsReady_ReturnsHealthyStatus()
    {
        // Act
        var response = await _fixture.WebClient.GetAsync("/api/diagnostics/ready");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("healthy");
    }

    [Fact]
    public async Task DiagnosticsInfo_ReturnsApplicationInfo()
    {
        // Act
        var response = await _fixture.WebClient.GetAsync("/api/diagnostics/info");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("CoralLedger Blue");
    }

    [Fact]
    public async Task DiagnosticsChecks_ReturnsCheckCategories()
    {
        // Act
        var response = await _fixture.WebClient.GetAsync("/api/diagnostics/checks");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("frontend");
        content.Should().Contain("realtime");
        content.Should().Contain("performance");
    }

    [Fact]
    public async Task BlazorFramework_IsAccessible()
    {
        // Act
        var response = await _fixture.WebClient.GetAsync("/_framework/blazor.web.js");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/javascript");
    }

    [Fact]
    public async Task OpenApi_DocumentIsAccessible()
    {
        // Act
        var response = await _fixture.WebClient.GetAsync("/openapi/v1.json");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }
}
