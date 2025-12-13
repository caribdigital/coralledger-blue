using CoralLedger.Blue.Infrastructure.ExternalServices;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CoralLedger.Blue.Infrastructure.Tests.ExternalServices;

public class ProtectedPlanetClientTests
{
    [Fact]
    public void Constructor_WithEnabledAndNoToken_LogsWarning()
    {
        // Arrange
        var options = Options.Create(new ProtectedPlanetOptions
        {
            Enabled = true,
            ApiToken = "" // Empty token
        });

        var mockLogger = new Mock<ILogger<ProtectedPlanetClient>>();
        var httpClient = new HttpClient();

        // Act
        var client = new ProtectedPlanetClient(httpClient, options, mockLogger.Object);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ProtectedPlanet is enabled but ApiToken is not configured")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Constructor_WithEnabledAndValidToken_DoesNotLogWarning()
    {
        // Arrange
        var options = Options.Create(new ProtectedPlanetOptions
        {
            Enabled = true,
            ApiToken = "valid-token-here"
        });

        var mockLogger = new Mock<ILogger<ProtectedPlanetClient>>();
        var httpClient = new HttpClient();

        // Act
        var client = new ProtectedPlanetClient(httpClient, options, mockLogger.Object);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public void Constructor_WithDisabledAndNoToken_DoesNotLogWarning()
    {
        // Arrange
        var options = Options.Create(new ProtectedPlanetOptions
        {
            Enabled = false,
            ApiToken = "" // Empty token, but disabled
        });

        var mockLogger = new Mock<ILogger<ProtectedPlanetClient>>();
        var httpClient = new HttpClient();

        // Act
        var client = new ProtectedPlanetClient(httpClient, options, mockLogger.Object);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public void Constructor_SetsBaseAddressCorrectly()
    {
        // Arrange
        var options = Options.Create(new ProtectedPlanetOptions
        {
            Enabled = true,
            ApiToken = "test-token"
        });

        var mockLogger = new Mock<ILogger<ProtectedPlanetClient>>();
        var httpClient = new HttpClient();

        // Act
        var client = new ProtectedPlanetClient(httpClient, options, mockLogger.Object);

        // Assert
        httpClient.BaseAddress.Should().Be(new Uri("https://api.protectedplanet.net/v4/"));
    }

    [Fact]
    public void Constructor_SetsAcceptHeader()
    {
        // Arrange
        var options = Options.Create(new ProtectedPlanetOptions
        {
            Enabled = true,
            ApiToken = "test-token"
        });

        var mockLogger = new Mock<ILogger<ProtectedPlanetClient>>();
        var httpClient = new HttpClient();

        // Act
        var client = new ProtectedPlanetClient(httpClient, options, mockLogger.Object);

        // Assert
        httpClient.DefaultRequestHeaders.Accept.Should().NotBeEmpty();
    }

    [Fact]
    public void IsConfigured_WithToken_ReturnsTrue()
    {
        // Arrange
        var options = Options.Create(new ProtectedPlanetOptions
        {
            Enabled = true,
            ApiToken = "valid-token"
        });

        var mockLogger = new Mock<ILogger<ProtectedPlanetClient>>();
        var httpClient = new HttpClient();
        var client = new ProtectedPlanetClient(httpClient, options, mockLogger.Object);

        // Act & Assert
        client.IsConfigured.Should().BeTrue();
    }

    [Fact]
    public void IsConfigured_WithoutToken_ReturnsFalse()
    {
        // Arrange
        var options = Options.Create(new ProtectedPlanetOptions
        {
            Enabled = true,
            ApiToken = ""
        });

        var mockLogger = new Mock<ILogger<ProtectedPlanetClient>>();
        var httpClient = new HttpClient();
        var client = new ProtectedPlanetClient(httpClient, options, mockLogger.Object);

        // Act & Assert
        client.IsConfigured.Should().BeFalse();
    }

    [Fact]
    public async Task GetProtectedAreaAsync_WithoutToken_ReturnsNull()
    {
        // Arrange
        var options = Options.Create(new ProtectedPlanetOptions
        {
            Enabled = true,
            ApiToken = "" // No token
        });

        var mockLogger = new Mock<ILogger<ProtectedPlanetClient>>();
        var httpClient = new HttpClient();
        var client = new ProtectedPlanetClient(httpClient, options, mockLogger.Object);

        // Act
        var result = await client.GetProtectedAreaAsync("12345");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SearchByCountryAsync_WithoutToken_ReturnsEmptyResult()
    {
        // Arrange
        var options = Options.Create(new ProtectedPlanetOptions
        {
            Enabled = true,
            ApiToken = "" // No token
        });

        var mockLogger = new Mock<ILogger<ProtectedPlanetClient>>();
        var httpClient = new HttpClient();
        var client = new ProtectedPlanetClient(httpClient, options, mockLogger.Object);

        // Act
        var result = await client.SearchByCountryAsync("BHS");

        // Assert
        result.Should().NotBeNull();
        result.ProtectedAreas.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }
}

public class ProtectedPlanetOptionsTests
{
    [Fact]
    public void SectionName_IsCorrect()
    {
        // Assert
        ProtectedPlanetOptions.SectionName.Should().Be("ProtectedPlanet");
    }

    [Fact]
    public void DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var options = new ProtectedPlanetOptions();

        // Assert
        options.ApiToken.Should().BeEmpty();
        options.Enabled.Should().BeTrue();
        options.BaseUrl.Should().Be("https://api.protectedplanet.net/v4/");
    }
}
