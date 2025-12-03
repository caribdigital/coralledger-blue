using NetTopologySuite.Geometries;

namespace CoralLedger.Application.Common.Interfaces;

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
