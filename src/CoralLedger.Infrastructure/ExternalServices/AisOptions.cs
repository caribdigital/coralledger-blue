namespace CoralLedger.Infrastructure.ExternalServices;

/// <summary>
/// Configuration options for AIS (Automatic Identification System) data providers
/// </summary>
public class AisOptions
{
    public const string SectionName = "AIS";

    /// <summary>
    /// Whether AIS integration is enabled
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// AIS data provider (MarineTraffic, AISHub, VesselFinder)
    /// </summary>
    public string Provider { get; set; } = "MarineTraffic";

    /// <summary>
    /// API key for the AIS provider
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Base URL for API (provider-specific)
    /// </summary>
    public string BaseUrl { get; set; } = "https://services.marinetraffic.com/api/";

    /// <summary>
    /// Update interval in seconds for vessel positions
    /// </summary>
    public int UpdateIntervalSeconds { get; set; } = 300; // 5 minutes

    /// <summary>
    /// Bounding box for vessel tracking (Bahamas area)
    /// </summary>
    public BoundingBox BoundingBox { get; set; } = new()
    {
        MinLon = -80.5,
        MaxLon = -72.5,
        MinLat = 20.5,
        MaxLat = 27.5
    };
}

public class BoundingBox
{
    public double MinLon { get; set; }
    public double MaxLon { get; set; }
    public double MinLat { get; set; }
    public double MaxLat { get; set; }
}
