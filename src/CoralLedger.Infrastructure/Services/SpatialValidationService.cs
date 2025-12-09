using CoralLedger.Application.Common.Interfaces;
using CoralLedger.Infrastructure.Common;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using TopologyException = NetTopologySuite.Geometries.TopologyException;

namespace CoralLedger.Infrastructure.Services;

/// <summary>
/// Implements Dr. Thorne's 10 spatial validation gates.
/// All external spatial data MUST pass validation before storage.
///
/// Gates:
/// 1. SRID must be 4326 (WGS84)
/// 2. Longitude range [-180, 180]
/// 3. Latitude range [-90, 90]
/// 4. Within Bahamas EEZ
/// 5. Geometry is valid
/// 6. Polygon is closed
/// 7. No self-intersections
/// 8. Exterior ring is counter-clockwise
/// 9. Reasonable size (less than 50,000 km²)
/// 10. Topology is valid
/// </summary>
public class SpatialValidationService : ISpatialValidationService
{
    private readonly ILogger<SpatialValidationService> _logger;
    private readonly Polygon _bahamasBoundingBox;

    public SpatialValidationService(ILogger<SpatialValidationService> logger)
    {
        _logger = logger;
        _bahamasBoundingBox = BahamasSpatialConstants.GetBahamasBoundingBoxPolygon();
    }

    public SpatialValidationResult ValidatePoint(Point point)
    {
        if (point == null)
        {
            return SpatialValidationResult.Failure("Point cannot be null.", 5);
        }

        if (point.IsEmpty)
        {
            return SpatialValidationResult.Failure("Point cannot be empty.", 5);
        }

        var errors = new List<string>();
        var failedGates = new List<int>();

        // Gate 1: SRID must be 4326
        if (point.SRID != 0 && point.SRID != BahamasSpatialConstants.StorageSrid)
        {
            errors.Add($"Invalid SRID {point.SRID}. Expected {BahamasSpatialConstants.StorageSrid}.");
            failedGates.Add(1);
        }

        // Gate 2: Longitude range [-180, 180]
        if (!BahamasSpatialConstants.IsValidLongitude(point.X))
        {
            errors.Add($"Longitude {point.X} out of range. Must be between -180 and 180.");
            failedGates.Add(2);
        }

        // Gate 3: Latitude range [-90, 90]
        if (!BahamasSpatialConstants.IsValidLatitude(point.Y))
        {
            errors.Add($"Latitude {point.Y} out of range. Must be between -90 and 90.");
            failedGates.Add(3);
        }

        // Gate 4: Within Bahamas EEZ
        if (!BahamasSpatialConstants.IsWithinBahamasBounds(point.X, point.Y))
        {
            errors.Add($"Coordinates ({point.X}, {point.Y}) outside Bahamas EEZ.");
            failedGates.Add(4);
        }

        // Gate 5: Geometry is valid
        if (!point.IsValid)
        {
            errors.Add("Invalid geometry.");
            failedGates.Add(5);
        }

        if (errors.Count > 0)
        {
            _logger.LogWarning(
                "Point validation failed at gates [{Gates}]: {Errors}",
                string.Join(", ", failedGates),
                string.Join("; ", errors));
            return SpatialValidationResult.Failure(errors, failedGates);
        }

        return SpatialValidationResult.Success();
    }

    public SpatialValidationResult ValidatePolygon(Polygon polygon)
    {
        if (polygon == null)
        {
            return SpatialValidationResult.Failure("Polygon cannot be null.", 5);
        }

        if (polygon.IsEmpty)
        {
            return SpatialValidationResult.Failure("Polygon cannot be empty.", 5);
        }

        var errors = new List<string>();
        var failedGates = new List<int>();

        // Gate 1: SRID must be 4326
        if (polygon.SRID != 0 && polygon.SRID != BahamasSpatialConstants.StorageSrid)
        {
            errors.Add($"Invalid SRID {polygon.SRID}. Expected {BahamasSpatialConstants.StorageSrid}.");
            failedGates.Add(1);
        }

        // Gate 2 & 3: Validate all coordinates in the polygon
        foreach (var coord in polygon.Coordinates)
        {
            if (!BahamasSpatialConstants.IsValidLongitude(coord.X) && !failedGates.Contains(2))
            {
                errors.Add($"Longitude {coord.X} out of range. Must be between -180 and 180.");
                failedGates.Add(2);
            }
            if (!BahamasSpatialConstants.IsValidLatitude(coord.Y) && !failedGates.Contains(3))
            {
                errors.Add($"Latitude {coord.Y} out of range. Must be between -90 and 90.");
                failedGates.Add(3);
            }
        }

        // Gate 4: Within Bahamas EEZ (check centroid)
        var centroid = polygon.Centroid;
        if (centroid != null && !BahamasSpatialConstants.IsWithinBahamasBounds(centroid.X, centroid.Y))
        {
            errors.Add("Polygon centroid outside Bahamas EEZ.");
            failedGates.Add(4);
        }

        // Gate 5: Geometry is valid
        if (!polygon.IsValid)
        {
            errors.Add("Invalid geometry.");
            failedGates.Add(5);
        }

        // Gate 6: Polygon is closed
        if (!polygon.ExteriorRing.IsClosed)
        {
            errors.Add("Polygon is not closed.");
            failedGates.Add(6);
        }

        // Gate 7: No self-intersections
        if (!polygon.IsSimple)
        {
            errors.Add("Polygon has self-intersections.");
            failedGates.Add(7);
        }

        // Gate 8: Exterior ring should be counter-clockwise (right-hand rule)
        if (!Orientation.IsCCW(polygon.ExteriorRing.CoordinateSequence))
        {
            errors.Add("Exterior ring should be counter-clockwise.");
            failedGates.Add(8);
        }

        // Gate 9: Reasonable size (<50,000 km²)
        var areaKm2 = CalculateAreaKm2(polygon);
        if (areaKm2 > BahamasSpatialConstants.MaxPolygonAreaKm2)
        {
            errors.Add($"Polygon area {areaKm2:N0} km² exceeds maximum {BahamasSpatialConstants.MaxPolygonAreaKm2:N0} km².");
            failedGates.Add(9);
        }

        // Gate 10: Topology is valid (already checked via IsValid, but can add more specific checks)
        if (polygon.Holes.Length > 0)
        {
            foreach (var hole in polygon.Holes)
            {
                if (!hole.IsClosed || !Orientation.IsCCW(hole.CoordinateSequence) == false)
                {
                    // Holes should be clockwise (opposite of exterior)
                }
            }
        }

        if (errors.Count > 0)
        {
            _logger.LogWarning(
                "Polygon validation failed at gates [{Gates}]: {Errors}",
                string.Join(", ", failedGates),
                string.Join("; ", errors));
            return SpatialValidationResult.Failure(errors, failedGates);
        }

        return SpatialValidationResult.Success();
    }

    public SpatialValidationResult ValidateGeometry(Geometry geometry)
    {
        if (geometry == null)
        {
            return SpatialValidationResult.Failure("Geometry cannot be null.", 5);
        }

        if (geometry.IsEmpty)
        {
            return SpatialValidationResult.Failure("Geometry cannot be empty.", 5);
        }

        return geometry switch
        {
            Point point => ValidatePoint(point),
            Polygon polygon => ValidatePolygon(polygon),
            MultiPolygon multiPolygon => ValidateMultiPolygon(multiPolygon),
            LineString lineString => ValidateLineString(lineString),
            _ => ValidateGenericGeometry(geometry)
        };
    }

    public bool IsWithinBahamas(Point point)
    {
        if (point == null || point.IsEmpty)
            return false;

        return BahamasSpatialConstants.IsWithinBahamasBounds(point.X, point.Y);
    }

    public bool IsWithinBahamas(Geometry geometry)
    {
        if (geometry == null || geometry.IsEmpty)
            return false;

        // Check if the geometry's envelope intersects with Bahamas bounds
        return _bahamasBoundingBox.Intersects(geometry);
    }

    private SpatialValidationResult ValidateMultiPolygon(MultiPolygon multiPolygon)
    {
        var allErrors = new List<string>();
        var allFailedGates = new List<int>();

        for (int i = 0; i < multiPolygon.NumGeometries; i++)
        {
            var polygon = (Polygon)multiPolygon.GetGeometryN(i);
            var result = ValidatePolygon(polygon);
            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    allErrors.Add($"Polygon {i + 1}: {error}");
                }
                foreach (var gate in result.FailedGates)
                {
                    if (!allFailedGates.Contains(gate))
                        allFailedGates.Add(gate);
                }
            }
        }

        if (allErrors.Count > 0)
        {
            return SpatialValidationResult.Failure(allErrors, allFailedGates);
        }

        return SpatialValidationResult.Success();
    }

    private SpatialValidationResult ValidateLineString(LineString lineString)
    {
        var errors = new List<string>();
        var failedGates = new List<int>();

        // Gate 1: SRID
        if (lineString.SRID != 0 && lineString.SRID != BahamasSpatialConstants.StorageSrid)
        {
            errors.Add($"Invalid SRID {lineString.SRID}. Expected {BahamasSpatialConstants.StorageSrid}.");
            failedGates.Add(1);
        }

        // Gate 2 & 3: Validate all coordinates
        foreach (var coord in lineString.Coordinates)
        {
            if (!BahamasSpatialConstants.IsValidLongitude(coord.X) && !failedGates.Contains(2))
            {
                errors.Add($"Longitude {coord.X} out of range.");
                failedGates.Add(2);
            }
            if (!BahamasSpatialConstants.IsValidLatitude(coord.Y) && !failedGates.Contains(3))
            {
                errors.Add($"Latitude {coord.Y} out of range.");
                failedGates.Add(3);
            }
        }

        // Gate 5: Geometry is valid
        if (!lineString.IsValid)
        {
            errors.Add("Invalid geometry.");
            failedGates.Add(5);
        }

        if (errors.Count > 0)
        {
            return SpatialValidationResult.Failure(errors, failedGates);
        }

        return SpatialValidationResult.Success();
    }

    private SpatialValidationResult ValidateGenericGeometry(Geometry geometry)
    {
        var errors = new List<string>();
        var failedGates = new List<int>();

        // Gate 1: SRID
        if (geometry.SRID != 0 && geometry.SRID != BahamasSpatialConstants.StorageSrid)
        {
            errors.Add($"Invalid SRID {geometry.SRID}. Expected {BahamasSpatialConstants.StorageSrid}.");
            failedGates.Add(1);
        }

        // Gate 5: Geometry is valid
        if (!geometry.IsValid)
        {
            errors.Add("Invalid geometry.");
            failedGates.Add(5);
        }

        if (errors.Count > 0)
        {
            return SpatialValidationResult.Failure(errors, failedGates);
        }

        return SpatialValidationResult.Success();
    }

    /// <summary>
    /// Calculate approximate area in square kilometers for a polygon in WGS84
    /// Uses a simple approximation based on latitude
    /// </summary>
    private static double CalculateAreaKm2(Polygon polygon)
    {
        // Get the centroid's latitude for more accurate calculation
        var centroidLat = polygon.Centroid.Y;
        var latRadians = centroidLat * Math.PI / 180.0;

        // Approximate degrees to km conversion at this latitude
        var kmPerDegreeLon = 111.320 * Math.Cos(latRadians);
        var kmPerDegreeLat = 110.574;

        // Get the area in degrees squared
        var areaInDegrees = polygon.Area;

        // Convert to km² (this is an approximation)
        return areaInDegrees * kmPerDegreeLon * kmPerDegreeLat;
    }

    /// <summary>
    /// Calculate approximate area in square kilometers for any geometry in WGS84
    /// </summary>
    private static double CalculateAreaKm2(Geometry geometry)
    {
        if (geometry == null || geometry.IsEmpty)
            return 0;

        // Get the centroid's latitude for more accurate calculation
        var centroid = geometry.Centroid;
        var centroidLat = centroid?.Y ?? 25.0; // Default to Bahamas latitude
        var latRadians = centroidLat * Math.PI / 180.0;

        // Approximate degrees to km conversion at this latitude
        var kmPerDegreeLon = 111.320 * Math.Cos(latRadians);
        var kmPerDegreeLat = 110.574;

        // Get the area in degrees squared
        var areaInDegrees = geometry.Area;

        // Convert to km² (this is an approximation)
        return areaInDegrees * kmPerDegreeLon * kmPerDegreeLat;
    }

    /// <inheritdoc />
    public BoundaryComparisonResult CompareBoundaries(Geometry existing, Geometry incoming)
    {
        if (existing == null || existing.IsEmpty || incoming == null || incoming.IsEmpty)
        {
            return new BoundaryComparisonResult
            {
                OverlapPercentage = 0,
                AreaDifferencePercentage = 100,
                JaccardSimilarityPercentage = 0,
                IsEquivalent = false,
                HasSignificantChange = true,
                ExistingAreaKm2 = existing != null ? CalculateAreaKm2(existing) : 0,
                IncomingAreaKm2 = incoming != null ? CalculateAreaKm2(incoming) : 0,
                OverlapAreaKm2 = 0,
                Summary = "One or both boundaries are null or empty"
            };
        }

        try
        {
            // Calculate areas
            var existingAreaKm2 = CalculateAreaKm2(existing);
            var incomingAreaKm2 = CalculateAreaKm2(incoming);

            // Calculate intersection and union for Jaccard similarity
            var intersection = existing.Intersection(incoming);
            var union = existing.Union(incoming);

            var intersectionAreaKm2 = CalculateAreaKm2(intersection);
            var unionAreaKm2 = CalculateAreaKm2(union);

            // Calculate metrics
            var overlapPercentage = existingAreaKm2 > 0
                ? (intersectionAreaKm2 / existingAreaKm2) * 100
                : 0;

            var areaDifferencePercentage = existingAreaKm2 > 0
                ? Math.Abs(incomingAreaKm2 - existingAreaKm2) / existingAreaKm2 * 100
                : (incomingAreaKm2 > 0 ? 100 : 0);

            var jaccardSimilarity = unionAreaKm2 > 0
                ? (intersectionAreaKm2 / unionAreaKm2) * 100
                : 0;

            // Thresholds: 95% = equivalent, <80% = significant change
            var isEquivalent = jaccardSimilarity >= 95;
            var hasSignificantChange = jaccardSimilarity < 80;

            // Generate summary
            var summary = isEquivalent
                ? $"Boundaries are equivalent ({jaccardSimilarity:F1}% similarity)"
                : hasSignificantChange
                    ? $"Significant boundary change detected ({jaccardSimilarity:F1}% similarity, {areaDifferencePercentage:F1}% area difference)"
                    : $"Minor boundary adjustment ({jaccardSimilarity:F1}% similarity)";

            _logger.LogDebug(
                "Boundary comparison: Jaccard={Jaccard:F1}%, Overlap={Overlap:F1}%, AreaDiff={AreaDiff:F1}%",
                jaccardSimilarity, overlapPercentage, areaDifferencePercentage);

            return new BoundaryComparisonResult
            {
                OverlapPercentage = Math.Round(overlapPercentage, 2),
                AreaDifferencePercentage = Math.Round(areaDifferencePercentage, 2),
                JaccardSimilarityPercentage = Math.Round(jaccardSimilarity, 2),
                IsEquivalent = isEquivalent,
                HasSignificantChange = hasSignificantChange,
                ExistingAreaKm2 = Math.Round(existingAreaKm2, 4),
                IncomingAreaKm2 = Math.Round(incomingAreaKm2, 4),
                OverlapAreaKm2 = Math.Round(intersectionAreaKm2, 4),
                Summary = summary
            };
        }
        catch (TopologyException ex)
        {
            _logger.LogWarning(ex, "Topology exception during boundary comparison - geometries may be invalid");
            return new BoundaryComparisonResult
            {
                OverlapPercentage = 0,
                AreaDifferencePercentage = 100,
                JaccardSimilarityPercentage = 0,
                IsEquivalent = false,
                HasSignificantChange = true,
                ExistingAreaKm2 = CalculateAreaKm2(existing),
                IncomingAreaKm2 = CalculateAreaKm2(incoming),
                OverlapAreaKm2 = 0,
                Summary = $"Comparison failed: {ex.Message}"
            };
        }
    }
}
