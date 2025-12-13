using System.Globalization;
using NetTopologySuite.Geometries;

namespace CoralLedger.Blue.Infrastructure.Common;

/// <summary>
/// Enforces Dr. Thorne's Precision Doctrine (Rule 7) for spatial data formatting.
/// Ensures consistent precision across all API responses and displays.
///
/// Precision Standards:
/// - Coordinates: 5 decimal places (~1.1m precision at equator)
/// - Temperature: 1 decimal place (tenths of degrees)
/// - Area: 2 decimal places (km²)
/// - Distance: 2 decimal places (km) or 0 decimal (meters)
/// </summary>
public static class SpatialFormatting
{
    /// <summary>
    /// Coordinate precision: 5 decimal places (~1.1 meters at equator, ~1.0m at 25°N)
    /// </summary>
    public const int CoordinatePrecision = 5;

    /// <summary>
    /// Temperature precision: 1 decimal place (tenths of degrees)
    /// </summary>
    public const int TemperaturePrecision = 1;

    /// <summary>
    /// Area precision: 2 decimal places for km²
    /// </summary>
    public const int AreaPrecision = 2;

    /// <summary>
    /// Distance precision: 2 decimal places for km
    /// </summary>
    public const int DistancePrecision = 2;

    /// <summary>
    /// Degree-Heating-Week (DHW) precision: 1 decimal place
    /// </summary>
    public const int DhwPrecision = 1;

    /// <summary>
    /// Percentage precision: 1 decimal place
    /// </summary>
    public const int PercentagePrecision = 1;

    #region Coordinate Formatting

    /// <summary>
    /// Format longitude to 5 decimal places.
    /// </summary>
    public static string FormatLongitude(double longitude)
    {
        var formatted = Math.Round(longitude, CoordinatePrecision);
        return formatted.ToString($"F{CoordinatePrecision}", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Format latitude to 5 decimal places.
    /// </summary>
    public static string FormatLatitude(double latitude)
    {
        var formatted = Math.Round(latitude, CoordinatePrecision);
        return formatted.ToString($"F{CoordinatePrecision}", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Format coordinates as "lon, lat" string.
    /// </summary>
    public static string FormatCoordinates(double longitude, double latitude)
    {
        return $"{FormatLongitude(longitude)}, {FormatLatitude(latitude)}";
    }

    /// <summary>
    /// Format Point geometry coordinates.
    /// </summary>
    public static string FormatPoint(Point point)
    {
        return FormatCoordinates(point.X, point.Y);
    }

    /// <summary>
    /// Round coordinate to standard precision.
    /// </summary>
    public static double RoundCoordinate(double coordinate)
    {
        return Math.Round(coordinate, CoordinatePrecision);
    }

    /// <summary>
    /// Create a Point with coordinates rounded to standard precision.
    /// </summary>
    public static Point CreatePrecisionPoint(double longitude, double latitude, int srid = 4326)
    {
        return new Point(
            RoundCoordinate(longitude),
            RoundCoordinate(latitude))
        { SRID = srid };
    }

    #endregion

    #region Temperature Formatting

    /// <summary>
    /// Format temperature to 1 decimal place.
    /// </summary>
    public static string FormatTemperature(double temperature)
    {
        return Math.Round(temperature, TemperaturePrecision)
            .ToString($"F{TemperaturePrecision}", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Format temperature with unit suffix.
    /// </summary>
    public static string FormatTemperatureWithUnit(double temperature, string unit = "C")
    {
        return $"{FormatTemperature(temperature)}°{unit}";
    }

    /// <summary>
    /// Round temperature to standard precision.
    /// </summary>
    public static double RoundTemperature(double temperature)
    {
        return Math.Round(temperature, TemperaturePrecision);
    }

    #endregion

    #region DHW (Degree-Heating-Week) Formatting

    /// <summary>
    /// Format DHW value to 1 decimal place.
    /// </summary>
    public static string FormatDhw(double dhw)
    {
        return Math.Round(dhw, DhwPrecision)
            .ToString($"F{DhwPrecision}", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Format DHW with unit suffix.
    /// </summary>
    public static string FormatDhwWithUnit(double dhw)
    {
        return $"{FormatDhw(dhw)} °C-weeks";
    }

    /// <summary>
    /// Round DHW to standard precision.
    /// </summary>
    public static double RoundDhw(double dhw)
    {
        return Math.Round(dhw, DhwPrecision);
    }

    #endregion

    #region Area Formatting

    /// <summary>
    /// Format area in km² to 2 decimal places.
    /// </summary>
    public static string FormatAreaKm2(double areaKm2)
    {
        return Math.Round(areaKm2, AreaPrecision)
            .ToString($"F{AreaPrecision}", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Format area with unit suffix.
    /// </summary>
    public static string FormatAreaWithUnit(double areaKm2)
    {
        return $"{FormatAreaKm2(areaKm2)} km²";
    }

    /// <summary>
    /// Round area to standard precision.
    /// </summary>
    public static double RoundArea(double areaKm2)
    {
        return Math.Round(areaKm2, AreaPrecision);
    }

    #endregion

    #region Distance Formatting

    /// <summary>
    /// Format distance in km to 2 decimal places.
    /// </summary>
    public static string FormatDistanceKm(double distanceKm)
    {
        return Math.Round(distanceKm, DistancePrecision)
            .ToString($"F{DistancePrecision}", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Format distance with unit suffix, automatically selecting km or m.
    /// </summary>
    public static string FormatDistanceWithUnit(double distanceKm)
    {
        if (distanceKm < 1.0)
        {
            var meters = (int)Math.Round(distanceKm * 1000);
            return $"{meters} m";
        }
        return $"{FormatDistanceKm(distanceKm)} km";
    }

    /// <summary>
    /// Round distance to standard precision.
    /// </summary>
    public static double RoundDistance(double distanceKm)
    {
        return Math.Round(distanceKm, DistancePrecision);
    }

    /// <summary>
    /// Format distance in meters (whole numbers).
    /// </summary>
    public static string FormatDistanceMeters(double meters)
    {
        return ((int)Math.Round(meters)).ToString(CultureInfo.InvariantCulture);
    }

    #endregion

    #region Percentage Formatting

    /// <summary>
    /// Format percentage to 1 decimal place.
    /// </summary>
    public static string FormatPercentage(double percentage)
    {
        return Math.Round(percentage, PercentagePrecision)
            .ToString($"F{PercentagePrecision}", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Format percentage with % suffix.
    /// </summary>
    public static string FormatPercentageWithSymbol(double percentage)
    {
        return $"{FormatPercentage(percentage)}%";
    }

    /// <summary>
    /// Round percentage to standard precision.
    /// </summary>
    public static double RoundPercentage(double percentage)
    {
        return Math.Round(percentage, PercentagePrecision);
    }

    #endregion

    #region GeoJSON Formatting

    /// <summary>
    /// Round all coordinates in a geometry to standard precision.
    /// Used before serializing to GeoJSON to ensure consistent output.
    /// </summary>
    public static Geometry RoundGeometryCoordinates(Geometry geometry)
    {
        var coordinates = geometry.Coordinates
            .Select(c =>
            {
                var coord = new Coordinate(
                    RoundCoordinate(c.X),
                    RoundCoordinate(c.Y));
                if (!double.IsNaN(c.Z))
                {
                    coord.Z = Math.Round(c.Z, DistancePrecision);
                }
                return coord;
            })
            .ToArray();

        var factory = geometry.Factory ?? BahamasSpatialConstants.GeometryFactory;

        return geometry switch
        {
            Point => factory.CreatePoint(coordinates[0]),
            LinearRing => factory.CreateLinearRing(coordinates),  // Must be before LineString (subclass)
            LineString => factory.CreateLineString(coordinates),
            Polygon polygon => RoundPolygonCoordinates(polygon, factory),
            MultiPoint => factory.CreateMultiPoint(coordinates.Select(c => factory.CreatePoint(c)).ToArray()),
            MultiLineString mls => factory.CreateMultiLineString(
                Enumerable.Range(0, mls.NumGeometries)
                    .Select(i => (LineString)RoundGeometryCoordinates(mls.GetGeometryN(i)))
                    .ToArray()),
            MultiPolygon mp => factory.CreateMultiPolygon(
                Enumerable.Range(0, mp.NumGeometries)
                    .Select(i => (Polygon)RoundGeometryCoordinates(mp.GetGeometryN(i)))
                    .ToArray()),
            GeometryCollection gc => factory.CreateGeometryCollection(
                Enumerable.Range(0, gc.NumGeometries)
                    .Select(i => RoundGeometryCoordinates(gc.GetGeometryN(i)))
                    .ToArray()),
            _ => geometry
        };
    }

    private static Polygon RoundPolygonCoordinates(Polygon polygon, GeometryFactory factory)
    {
        // Round shell coordinates
        var shellCoords = polygon.Shell.Coordinates
            .Select(c => new Coordinate(RoundCoordinate(c.X), RoundCoordinate(c.Y)))
            .ToArray();
        var shell = factory.CreateLinearRing(shellCoords);

        // Round hole coordinates
        var holes = new LinearRing[polygon.NumInteriorRings];
        for (int i = 0; i < polygon.NumInteriorRings; i++)
        {
            var holeCoords = polygon.GetInteriorRingN(i).Coordinates
                .Select(c => new Coordinate(RoundCoordinate(c.X), RoundCoordinate(c.Y)))
                .ToArray();
            holes[i] = factory.CreateLinearRing(holeCoords);
        }

        return factory.CreatePolygon(shell, holes);
    }

    #endregion

    #region Display Formatting

    /// <summary>
    /// Format coordinates for human-readable display (e.g., "25.0480°N, 77.3554°W").
    /// </summary>
    public static string FormatCoordinatesForDisplay(double longitude, double latitude)
    {
        var latDirection = latitude >= 0 ? "N" : "S";
        var lonDirection = longitude >= 0 ? "E" : "W";

        return $"{Math.Abs(latitude):F4}°{latDirection}, {Math.Abs(longitude):F4}°{lonDirection}";
    }

    /// <summary>
    /// Format depth in meters for display.
    /// </summary>
    public static string FormatDepth(double depthMeters)
    {
        return $"{Math.Round(depthMeters, 1):F1}m";
    }

    /// <summary>
    /// Format a range of values (e.g., "24.5 - 28.3°C").
    /// </summary>
    public static string FormatTemperatureRange(double min, double max)
    {
        return $"{FormatTemperature(min)} - {FormatTemperature(max)}°C";
    }

    #endregion
}
