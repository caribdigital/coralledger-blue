namespace CoralLedger.Blue.Infrastructure.ExternalServices;

/// <summary>
/// Configuration options for Protected Planet WDPA API v4
/// Request an API token at: https://api.protectedplanet.net/request
/// </summary>
public class ProtectedPlanetOptions
{
    public const string SectionName = "ProtectedPlanet";

    /// <summary>
    /// API token for authentication (query parameter based)
    /// </summary>
    public string ApiToken { get; set; } = string.Empty;

    /// <summary>
    /// Whether to enable the API client (false for development without API key)
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Base URL for the Protected Planet API
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.protectedplanet.net/v4/";
}
