using NetTopologySuite.Geometries;

namespace CoralLedger.Infrastructure.Common;

/// <summary>
/// Defines Bahamas Exclusive Economic Zone (EEZ) boundaries and SRID constants
/// for validating all external spatial data.
///
/// Reference: Dr. Thorne's 10 GIS Commandments (Rules 1 and 2)
/// </summary>
public static class BahamasSpatialConstants
{
    /// <summary>
    /// WGS84 SRID for storage and API operations (Thorne Rule 1)
    /// </summary>
    public const int StorageSrid = 4326;

    /// <summary>
    /// UTM Zone 18N SRID for distance/area calculations in meters (Thorne Rule 1)
    /// </summary>
    public const int CalculationSrid = 32618;

    /// <summary>
    /// Western limit of Bahamas EEZ (Cay Sal Bank)
    /// </summary>
    public const double MinLongitude = -80.5;

    /// <summary>
    /// Eastern limit of Bahamas EEZ
    /// </summary>
    public const double MaxLongitude = -72.0;

    /// <summary>
    /// Southern limit of Bahamas EEZ (Inagua)
    /// </summary>
    public const double MinLatitude = 20.0;

    /// <summary>
    /// Northern limit of Bahamas EEZ
    /// </summary>
    public const double MaxLatitude = 28.0;

    /// <summary>
    /// Maximum allowed polygon area in square kilometers (50,000 km²)
    /// Larger than the entire Bahamas archipelago - prevents obviously erroneous geometries
    /// </summary>
    public const double MaxPolygonAreaKm2 = 50_000;

    /// <summary>
    /// Geometry factory for creating spatial objects with WGS84 SRID
    /// </summary>
    public static readonly GeometryFactory GeometryFactory = new(new PrecisionModel(), StorageSrid);

    /// <summary>
    /// Well-Known Text representation of the Bahamas EEZ bounding box
    /// </summary>
    public static string BoundingBoxWkt =>
        $"POLYGON(({MinLongitude} {MinLatitude}, {MaxLongitude} {MinLatitude}, " +
        $"{MaxLongitude} {MaxLatitude}, {MinLongitude} {MaxLatitude}, {MinLongitude} {MinLatitude}))";

    /// <summary>
    /// Check if coordinates are within the Bahamas EEZ bounding box
    /// </summary>
    /// <param name="longitude">Longitude in WGS84</param>
    /// <param name="latitude">Latitude in WGS84</param>
    /// <returns>True if within bounds</returns>
    public static bool IsWithinBahamasBounds(double longitude, double latitude)
    {
        return longitude >= MinLongitude && longitude <= MaxLongitude &&
               latitude >= MinLatitude && latitude <= MaxLatitude;
    }

    /// <summary>
    /// Check if longitude is within valid WGS84 range
    /// </summary>
    public static bool IsValidLongitude(double longitude) =>
        longitude >= -180.0 && longitude <= 180.0;

    /// <summary>
    /// Check if latitude is within valid WGS84 range
    /// </summary>
    public static bool IsValidLatitude(double latitude) =>
        latitude >= -90.0 && latitude <= 90.0;

    /// <summary>
    /// Get the Bahamas EEZ bounding box as a Polygon
    /// </summary>
    public static Polygon GetBahamasBoundingBoxPolygon()
    {
        var coordinates = new[]
        {
            new Coordinate(MinLongitude, MinLatitude),
            new Coordinate(MaxLongitude, MinLatitude),
            new Coordinate(MaxLongitude, MaxLatitude),
            new Coordinate(MinLongitude, MaxLatitude),
            new Coordinate(MinLongitude, MinLatitude)
        };
        return GeometryFactory.CreatePolygon(coordinates);
    }

    /// <summary>
    /// Well-known Bahamas coordinates for testing and validation
    /// </summary>
    public static class WellKnownLocations
    {
        /// <summary>
        /// Nassau, New Providence: 25.0480° N, 77.3554° W
        /// </summary>
        public static Point Nassau => GeometryFactory.CreatePoint(new Coordinate(-77.3554, 25.0480));

        /// <summary>
        /// Exuma Cays Land and Sea Park centroid: 24.4667° N, 76.5833° W
        /// </summary>
        public static Point ExumaLandSeaPark => GeometryFactory.CreatePoint(new Coordinate(-76.5833, 24.4667));

        /// <summary>
        /// Andros Barrier Reef: 24.7° N, 78.0° W
        /// </summary>
        public static Point AndrosBarrierReef => GeometryFactory.CreatePoint(new Coordinate(-78.0, 24.7));

        /// <summary>
        /// Inagua (southern limit): 21.1° N, 73.3° W
        /// </summary>
        public static Point Inagua => GeometryFactory.CreatePoint(new Coordinate(-73.3, 21.1));

        /// <summary>
        /// Grand Bahama (northern area): 26.6° N, 78.5° W
        /// </summary>
        public static Point GrandBahama => GeometryFactory.CreatePoint(new Coordinate(-78.5, 26.6));
    }

    /// <summary>
    /// Coordinates outside Bahamas EEZ for testing rejection
    /// </summary>
    public static class OutOfBoundsLocations
    {
        /// <summary>
        /// Key West, Florida: 24.5551° N, 81.7800° W (west of Bahamas EEZ at -81.78)
        /// MinLongitude is -80.5, so this is outside
        /// </summary>
        public static Point KeyWest => GeometryFactory.CreatePoint(new Coordinate(-81.78, 24.5551));

        /// <summary>
        /// Havana, Cuba: 23.1136° N, 82.3666° W (south and west of Bahamas EEZ)
        /// </summary>
        public static Point Havana => GeometryFactory.CreatePoint(new Coordinate(-82.3666, 23.1136));

        /// <summary>
        /// Puerto Rico: 18.4655° N, 66.1057° W (east of Bahamas EEZ)
        /// </summary>
        public static Point PuertoRico => GeometryFactory.CreatePoint(new Coordinate(-66.1057, 18.4655));

        /// <summary>
        /// Jamaica: 18.1096° N, 77.2975° W (south of Bahamas EEZ at lat 18.1, MinLatitude is 20.0)
        /// </summary>
        public static Point Jamaica => GeometryFactory.CreatePoint(new Coordinate(-77.2975, 18.1096));
    }
}
