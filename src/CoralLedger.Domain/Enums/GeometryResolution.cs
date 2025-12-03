namespace CoralLedger.Domain.Enums;

/// <summary>
/// Geometry resolution levels for map display performance optimization
/// </summary>
public enum GeometryResolution
{
    /// <summary>
    /// Full original geometry with no simplification
    /// Use for: Detail views, data exports, precise calculations
    /// </summary>
    Full,

    /// <summary>
    /// Medium simplification (~0.001° tolerance, ~100m at 25°N latitude)
    /// Use for: Default map view, most interactive scenarios
    /// </summary>
    Medium,

    /// <summary>
    /// Low resolution (~0.01° tolerance, ~1km at 25°N latitude)
    /// Use for: Overview maps, thumbnails, low-bandwidth scenarios
    /// </summary>
    Low
}
