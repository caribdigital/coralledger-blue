using CoralLedger.Application.Common.Interfaces;
using CoralLedger.Infrastructure.Common;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

namespace CoralLedger.Infrastructure.Services;

/// <summary>
/// Provides accurate spatial calculations using proper SRID transformations.
/// Uses ProjNet for coordinate transformation when PostGIS raw SQL is not available.
/// Implements Dr. Thorne's GIS Rules:
/// - Rule 1: SRID 4326 for storage, SRID 32618 (UTM Zone 18N) for distance
/// - Rule 5: Geography calculations for spherical accuracy
/// </summary>
public class SpatialCalculator : ISpatialCalculator
{
    private readonly ILogger<SpatialCalculator> _logger;
    private readonly MathTransform? _wgs84ToUtm;
    private readonly GeometryFactory _utmFactory;

    // Earth radius in meters for spherical calculations
    private const double EarthRadiusMeters = 6371000.0;

    // UTM Zone 18N WKT definition for The Bahamas
    private const string Utm18NWkt = @"
        PROJCS[""WGS 84 / UTM zone 18N"",
            GEOGCS[""WGS 84"",
                DATUM[""WGS_1984"",
                    SPHEROID[""WGS 84"",6378137,298.257223563]],
                PRIMEM[""Greenwich"",0],
                UNIT[""degree"",0.0174532925199433]],
            PROJECTION[""Transverse_Mercator""],
            PARAMETER[""latitude_of_origin"",0],
            PARAMETER[""central_meridian"",-75],
            PARAMETER[""scale_factor"",0.9996],
            PARAMETER[""false_easting"",500000],
            PARAMETER[""false_northing"",0],
            UNIT[""metre"",1]]";

    public SpatialCalculator(ILogger<SpatialCalculator> logger)
    {
        _logger = logger;
        _utmFactory = new GeometryFactory(new PrecisionModel(), BahamasSpatialConstants.CalculationSrid);

        try
        {
            var csFactory = new CoordinateSystemFactory();
            var ctFactory = new CoordinateTransformationFactory();

            var wgs84 = GeographicCoordinateSystem.WGS84;
            var utm18N = csFactory.CreateFromWkt(Utm18NWkt);

            var transformation = ctFactory.CreateFromCoordinateSystems(wgs84, utm18N);
            _wgs84ToUtm = transformation.MathTransform;

            _logger.LogDebug("Initialized SpatialCalculator with ProjNet UTM Zone 18N transformation");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to initialize ProjNet transformation. Falling back to Haversine formula.");
            _wgs84ToUtm = null;
        }
    }

    public double CalculateDistanceMeters(Point point1, Point point2)
    {
        if (_wgs84ToUtm != null)
        {
            try
            {
                // Transform both points to UTM Zone 18N
                var utm1 = TransformToUtm(point1);
                var utm2 = TransformToUtm(point2);

                // Calculate Euclidean distance in meters
                return utm1.Distance(utm2);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "UTM transformation failed, falling back to Haversine");
            }
        }

        // Fallback to Haversine formula
        return CalculateHaversineDistance(point1.Y, point1.X, point2.Y, point2.X);
    }

    public double CalculateDistanceKm(Point point1, Point point2)
    {
        return CalculateDistanceMeters(point1, point2) / 1000.0;
    }

    public double CalculateDistanceToGeometryKm(Point point, Geometry geometry)
    {
        if (geometry.Contains(point))
        {
            return 0.0;
        }

        if (_wgs84ToUtm != null)
        {
            try
            {
                // Transform both geometries to UTM
                var utmPoint = TransformToUtm(point);
                var utmGeometry = TransformGeometryToUtm(geometry);

                // Calculate distance in meters
                var distanceMeters = utmPoint.Distance(utmGeometry);
                return distanceMeters / 1000.0;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "UTM transformation failed for geometry distance, using approximation");
            }
        }

        // Fallback: Use NTS distance in degrees and convert (approximate)
        var distanceDegrees = point.Distance(geometry);
        return distanceDegrees * 111.0; // Rough approximation at Bahamas latitude
    }

    public double CalculateAreaKm2(Polygon polygon)
    {
        return CalculateAreaKm2((Geometry)polygon);
    }

    public double CalculateAreaKm2(Geometry geometry)
    {
        if (geometry.IsEmpty)
        {
            return 0.0;
        }

        if (_wgs84ToUtm != null)
        {
            try
            {
                // Transform to UTM for accurate area calculation
                var utmGeometry = TransformGeometryToUtm(geometry);
                var areaM2 = utmGeometry.Area;
                return areaM2 / 1_000_000.0; // Convert m² to km²
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "UTM transformation failed for area calculation, using spherical approximation");
            }
        }

        // Fallback: Use spherical area calculation
        return CalculateSphericalAreaKm2(geometry);
    }

    public bool IsWithinDistance(Point point, Geometry geometry, double distanceMeters)
    {
        var distanceKm = CalculateDistanceToGeometryKm(point, geometry);
        return distanceKm * 1000.0 <= distanceMeters;
    }

    private Point TransformToUtm(Point point)
    {
        if (_wgs84ToUtm == null)
        {
            throw new InvalidOperationException("UTM transformation not initialized");
        }

        var coords = _wgs84ToUtm.Transform(new[] { point.X, point.Y });
        return _utmFactory.CreatePoint(new Coordinate(coords[0], coords[1]));
    }

    private Geometry TransformGeometryToUtm(Geometry geometry)
    {
        if (_wgs84ToUtm == null)
        {
            throw new InvalidOperationException("UTM transformation not initialized");
        }

        // Transform all coordinates
        var transformedCoords = geometry.Coordinates
            .Select(c =>
            {
                var coords = _wgs84ToUtm.Transform(new[] { c.X, c.Y });
                return new Coordinate(coords[0], coords[1]);
            })
            .ToArray();

        // Recreate the geometry in UTM CRS
        return geometry switch
        {
            Point => _utmFactory.CreatePoint(transformedCoords[0]),
            LineString => _utmFactory.CreateLineString(transformedCoords),
            Polygon polygon => CreateUtmPolygon(polygon, transformedCoords),
            MultiPolygon mp => CreateUtmMultiPolygon(mp),
            _ => _utmFactory.CreateGeometry(geometry)
        };
    }

    private Polygon CreateUtmPolygon(Polygon original, Coordinate[] allCoords)
    {
        // Shell coordinates
        var shellCoordCount = original.Shell.NumPoints;
        var shellCoords = allCoords.Take(shellCoordCount).ToArray();
        var shell = _utmFactory.CreateLinearRing(shellCoords);

        // Holes (if any)
        var holeOffset = shellCoordCount;
        var holes = new LinearRing[original.NumInteriorRings];
        for (int i = 0; i < original.NumInteriorRings; i++)
        {
            var holeCoordCount = original.GetInteriorRingN(i).NumPoints;
            var holeCoords = allCoords.Skip(holeOffset).Take(holeCoordCount).ToArray();
            holes[i] = _utmFactory.CreateLinearRing(holeCoords);
            holeOffset += holeCoordCount;
        }

        return _utmFactory.CreatePolygon(shell, holes);
    }

    private MultiPolygon CreateUtmMultiPolygon(MultiPolygon mp)
    {
        var polygons = new Polygon[mp.NumGeometries];
        for (int i = 0; i < mp.NumGeometries; i++)
        {
            var poly = (Polygon)mp.GetGeometryN(i);
            var coords = TransformGeometryToUtm(poly).Coordinates;
            polygons[i] = CreateUtmPolygon(poly, coords);
        }
        return _utmFactory.CreateMultiPolygon(polygons);
    }

    /// <summary>
    /// Calculate distance using Haversine formula.
    /// Accurate for any distance on Earth.
    /// </summary>
    private static double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadiusMeters * c;
    }

    /// <summary>
    /// Calculate area using spherical excess formula.
    /// Less accurate than UTM but works without transformation.
    /// </summary>
    private static double CalculateSphericalAreaKm2(Geometry geometry)
    {
        var coords = geometry.Coordinates;
        if (coords.Length < 3)
        {
            return 0.0;
        }

        // Spherical excess formula for polygon area
        double area = 0.0;
        for (int i = 0; i < coords.Length - 1; i++)
        {
            var p1 = coords[i];
            var p2 = coords[i + 1];

            // Use shoelace formula with latitude correction
            area += ToRadians(p2.X - p1.X) *
                    (2 + Math.Sin(ToRadians(p1.Y)) + Math.Sin(ToRadians(p2.Y)));
        }

        area = Math.Abs(area * EarthRadiusMeters * EarthRadiusMeters / 2.0);
        return area / 1_000_000.0; // Convert m² to km²
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;
}
