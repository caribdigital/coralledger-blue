using System.Net;
using Microsoft.AspNetCore.SignalR.Client;

namespace CoralLedger.Blue.Aspire.Tests.Tests;

/// <summary>
/// Tests for SignalR AlertHub connectivity
/// </summary>
[Collection("Aspire")]
public class SignalRHubTests
{
    private readonly AspireIntegrationFixture _fixture;

    public SignalRHubTests(AspireIntegrationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task SignalRHub_NegotiateEndpoint_IsAccessible()
    {
        // Act
        var response = await _fixture.WebClient.PostAsync("/hubs/alerts/negotiate?negotiateVersion=1", null);

        // Assert
        // SignalR negotiate can return 200 or 400 (bad request without proper handshake)
        // Both indicate the hub is configured and accessible
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SignalRHub_CanConnect()
    {
        // Arrange
        var hubUrl = $"{_fixture.WebBaseUrl}/hubs/alerts";

        var connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                // Accept any certificate for localhost testing
                options.HttpMessageHandlerFactory = _ => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (_, _, _, _) => true
                };
            })
            .Build();

        try
        {
            // Act
            await connection.StartAsync();

            // Assert
            connection.State.Should().Be(HubConnectionState.Connected);
        }
        finally
        {
            await connection.DisposeAsync();
        }
    }
}
