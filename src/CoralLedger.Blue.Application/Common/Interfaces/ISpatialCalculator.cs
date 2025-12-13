using NetTopologySuite.Geometries;

namespace CoralLedger.Blue.Application.Common.Interfaces;

/// <summary>
/// Provides accurate spatial calculations using proper SRID transformations.
/// Implements Dr. Thorne's GIS Rules 1, 5, and 7:
/// - Rule 1: SRID 4326 for storage, SRID 32618 (UTM Zone 18N) for distance calculations
/// - Rule 5: Geography type for spherical accuracy in area calculations
/// - Rule 7: Precision doctrine for coordinate formatting
/// </summary>
public interface ISpatialCalculator
{
    /// <summary>
    /// Calculate distance in meters between two points using UTM Zone 18N (SRID 32618).
    /// More accurate than simple Haversine at Bahamas latitude.
    /// </summary>
    /// <param name="point1">First point in WGS84 (SRID 4326)</param>
    /// <param name="point2">Second point in WGS84 (SRID 4326)</param>
    /// <returns>Distance in meters</returns>
    double CalculateDistanceMeters(Point point1, Point point2);

    /// <summary>
    /// Calculate distance in kilometers between two points.
    /// </summary>
    double CalculateDistanceKm(Point point1, Point point2);

    /// <summary>
    /// Calculate the distance from a point to the nearest edge of a geometry.
    /// Uses ST_Transform to SRID 32618 for accurate meter results.
    /// </summary>
    /// <param name="point">Point in WGS84</param>
    /// <param name="geometry">Geometry in WGS84</param>
    /// <returns>Distance in kilometers</returns>
    double CalculateDistanceToGeometryKm(Point point, Geometry geometry);

    /// <summary>
    /// Calculate the area of a polygon in square kilometers.
    /// Uses geography type for spherical accuracy (Thorne Rule 5).
    /// </summary>
    /// <param name="polygon">Polygon in WGS84 (SRID 4326)</param>
    /// <returns>Area in square kilometers</returns>
    double CalculateAreaKm2(Polygon polygon);

    /// <summary>
    /// Calculate the area of a geometry in square kilometers.
    /// </summary>
    double CalculateAreaKm2(Geometry geometry);

    /// <summary>
    /// Check if a point is within a specified distance of a geometry.
    /// Uses PostGIS ST_DWithin with geography type for accuracy.
    /// </summary>
    /// <param name="point">Point to check</param>
    /// <param name="geometry">Target geometry</param>
    /// <param name="distanceMeters">Buffer distance in meters</param>
    /// <returns>True if within distance</returns>
    bool IsWithinDistance(Point point, Geometry geometry, double distanceMeters);
}

/// <summary>
/// Extension methods for spatial calculations on geometry collections.
/// </summary>
public static class SpatialCalculatorExtensions
{
    /// <summary>
    /// Calculate the total area of multiple geometries.
    /// </summary>
    public static double CalculateTotalAreaKm2(this ISpatialCalculator calculator, IEnumerable<Geometry> geometries)
    {
        return geometries.Sum(g => calculator.CalculateAreaKm2(g));
    }
}
