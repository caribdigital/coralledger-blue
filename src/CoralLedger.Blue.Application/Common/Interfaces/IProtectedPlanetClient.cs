using NetTopologySuite.Geometries;

namespace CoralLedger.Blue.Application.Common.Interfaces;

/// <summary>
/// Client interface for Protected Planet WDPA API v4
/// https://api.protectedplanet.net/documentation
/// </summary>
public interface IProtectedPlanetClient
{
    /// <summary>
    /// Get a protected area by its WDPA ID including geometry
    /// </summary>
    /// <param name="wdpaId">The WDPA site ID (e.g., "305983")</param>
    /// <param name="withGeometry">Whether to include the boundary geometry</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Protected area data or null if not found</returns>
    Task<ProtectedAreaDto?> GetProtectedAreaAsync(
        string wdpaId,
        bool withGeometry = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Search for protected areas by country ISO3 code
    /// </summary>
    /// <param name="iso3Code">ISO3 country code (e.g., "BHS" for Bahamas)</param>
    /// <param name="withGeometry">Whether to include boundary geometries (larger response)</param>
    /// <param name="marineOnly">Only return marine protected areas</param>
    /// <param name="page">Page number for pagination</param>
    /// <param name="perPage">Results per page (max 50)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of protected areas matching the criteria</returns>
    Task<ProtectedAreaSearchResult> SearchByCountryAsync(
        string iso3Code,
        bool withGeometry = false,
        bool marineOnly = true,
        int page = 1,
        int perPage = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if the API client is configured with a valid token
    /// </summary>
    bool IsConfigured { get; }
}

/// <summary>
/// Protected area data from Protected Planet WDPA API
/// </summary>
public record ProtectedAreaDto
{
    /// <summary>WDPA site ID</summary>
    public string SiteId { get; init; } = string.Empty;

    /// <summary>Official name of the protected area</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>English name if different from official name</summary>
    public string? NameEnglish { get; init; }

    /// <summary>Original (local) name</summary>
    public string? OriginalName { get; init; }

    /// <summary>Total reported area in km²</summary>
    public double? ReportedArea { get; init; }

    /// <summary>GIS-calculated area in km²</summary>
    public double? GisArea { get; init; }

    /// <summary>Marine area in km² (for mixed terrestrial/marine sites)</summary>
    public double? GisMarineArea { get; init; }

    /// <summary>Designation type (e.g., "National Park", "Marine Reserve")</summary>
    public string? Designation { get; init; }

    /// <summary>IUCN management category (e.g., "II", "IV")</summary>
    public string? IucnCategory { get; init; }

    /// <summary>Governance type</summary>
    public string? Governance { get; init; }

    /// <summary>Management authority/organization</summary>
    public string? ManagementAuthority { get; init; }

    /// <summary>Year of legal designation</summary>
    public int? DesignationYear { get; init; }

    /// <summary>Whether the area is marine</summary>
    public bool IsMarine { get; init; }

    /// <summary>Status (e.g., "Designated", "Proposed")</summary>
    public string? Status { get; init; }

    /// <summary>ISO3 country code</summary>
    public string? CountryIso3 { get; init; }

    /// <summary>Boundary geometry (Polygon or MultiPolygon)</summary>
    public Geometry? Boundary { get; init; }
}

/// <summary>
/// Paginated search result from Protected Planet API
/// </summary>
public record ProtectedAreaSearchResult
{
    /// <summary>List of protected areas in this page</summary>
    public IReadOnlyList<ProtectedAreaDto> ProtectedAreas { get; init; } = [];

    /// <summary>Current page number</summary>
    public int CurrentPage { get; init; }

    /// <summary>Total number of pages</summary>
    public int TotalPages { get; init; }

    /// <summary>Total number of results across all pages</summary>
    public int TotalCount { get; init; }
}
