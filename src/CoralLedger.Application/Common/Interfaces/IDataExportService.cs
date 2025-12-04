namespace CoralLedger.Application.Common.Interfaces;

/// <summary>
/// Service for exporting marine data in various GIS formats
/// </summary>
public interface IDataExportService
{
    /// <summary>
    /// Export MPAs as GeoJSON FeatureCollection
    /// </summary>
    Task<string> ExportMpasAsGeoJsonAsync(ExportOptions? options = null, CancellationToken ct = default);

    /// <summary>
    /// Export vessel events as GeoJSON FeatureCollection
    /// </summary>
    Task<string> ExportVesselEventsAsGeoJsonAsync(ExportOptions? options = null, CancellationToken ct = default);

    /// <summary>
    /// Export bleaching alerts as GeoJSON FeatureCollection
    /// </summary>
    Task<string> ExportBleachingAlertsAsGeoJsonAsync(ExportOptions? options = null, CancellationToken ct = default);

    /// <summary>
    /// Export citizen observations as GeoJSON FeatureCollection
    /// </summary>
    Task<string> ExportObservationsAsGeoJsonAsync(ExportOptions? options = null, CancellationToken ct = default);

    /// <summary>
    /// Export MPAs as Shapefile (returns zip file bytes)
    /// </summary>
    Task<byte[]> ExportMpasAsShapefileAsync(ExportOptions? options = null, CancellationToken ct = default);

    /// <summary>
    /// Export data as CSV
    /// </summary>
    Task<string> ExportAsCsvAsync(ExportDataType dataType, ExportOptions? options = null, CancellationToken ct = default);
}

public class ExportOptions
{
    /// <summary>
    /// Filter by date range (start)
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// Filter by date range (end)
    /// </summary>
    public DateTime? ToDate { get; set; }

    /// <summary>
    /// Filter by specific MPA IDs
    /// </summary>
    public List<Guid>? MpaIds { get; set; }

    /// <summary>
    /// Filter by island group
    /// </summary>
    public string? IslandGroup { get; set; }

    /// <summary>
    /// Filter by protection level
    /// </summary>
    public string? ProtectionLevel { get; set; }

    /// <summary>
    /// Include related data (e.g., reefs for MPAs)
    /// </summary>
    public bool IncludeRelated { get; set; } = false;

    /// <summary>
    /// Maximum number of records to export
    /// </summary>
    public int? Limit { get; set; }
}

public enum ExportDataType
{
    MarineProtectedAreas,
    VesselEvents,
    BleachingAlerts,
    CitizenObservations,
    Reefs,
    Alerts
}
