using NetTopologySuite.Geometries;

namespace CoralLedger.Blue.Application.Common.Interfaces;

/// <summary>
/// Validates spatial data according to Dr. Thorne's 10 validation gates.
/// All external spatial data MUST pass through this service before storage.
/// </summary>
public interface ISpatialValidationService
{
    /// <summary>
    /// Validate a point geometry through all applicable gates
    /// </summary>
    SpatialValidationResult ValidatePoint(Point point);

    /// <summary>
    /// Validate a polygon geometry through all 10 gates
    /// </summary>
    SpatialValidationResult ValidatePolygon(Polygon polygon);

    /// <summary>
    /// Validate any geometry through applicable gates
    /// </summary>
    SpatialValidationResult ValidateGeometry(Geometry geometry);

    /// <summary>
    /// Check if a point is within the Bahamas EEZ bounding box
    /// </summary>
    bool IsWithinBahamas(Point point);

    /// <summary>
    /// Check if a geometry is within the Bahamas EEZ bounding box
    /// </summary>
    bool IsWithinBahamas(Geometry geometry);

    /// <summary>
    /// Compare two boundary geometries and calculate overlap metrics.
    /// Used for WDPA sync to detect significant boundary changes.
    /// </summary>
    /// <param name="existing">Current boundary in the database</param>
    /// <param name="incoming">New boundary from WDPA</param>
    /// <returns>Comparison result with overlap metrics</returns>
    BoundaryComparisonResult CompareBoundaries(Geometry existing, Geometry incoming);
}

/// <summary>
/// Result of comparing two boundary geometries
/// </summary>
public sealed record BoundaryComparisonResult
{
    /// <summary>Percentage of the existing boundary covered by the incoming boundary (0-100)</summary>
    public double OverlapPercentage { get; init; }

    /// <summary>Percentage difference in total area</summary>
    public double AreaDifferencePercentage { get; init; }

    /// <summary>Jaccard similarity index (intersection / union) as percentage (0-100)</summary>
    public double JaccardSimilarityPercentage { get; init; }

    /// <summary>Whether the boundaries are considered equivalent (Jaccard >= 95%)</summary>
    public bool IsEquivalent { get; init; }

    /// <summary>Whether there is a significant change (Jaccard < 80%)</summary>
    public bool HasSignificantChange { get; init; }

    /// <summary>Area of the existing boundary in km²</summary>
    public double ExistingAreaKm2 { get; init; }

    /// <summary>Area of the incoming boundary in km²</summary>
    public double IncomingAreaKm2 { get; init; }

    /// <summary>Area of overlap in km²</summary>
    public double OverlapAreaKm2 { get; init; }

    /// <summary>Description of the comparison result</summary>
    public string Summary { get; init; } = string.Empty;
}

/// <summary>
/// Result of spatial validation containing pass/fail status and any error messages
/// </summary>
public sealed record SpatialValidationResult
{
    public bool IsValid { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = [];
    public IReadOnlyList<int> FailedGates { get; init; } = [];

    public static SpatialValidationResult Success() =>
        new() { IsValid = true };

    public static SpatialValidationResult Failure(IReadOnlyList<string> errors, IReadOnlyList<int> failedGates) =>
        new() { IsValid = false, Errors = errors, FailedGates = failedGates };

    public static SpatialValidationResult Failure(string error, int gate) =>
        new() { IsValid = false, Errors = [error], FailedGates = [gate] };
}
