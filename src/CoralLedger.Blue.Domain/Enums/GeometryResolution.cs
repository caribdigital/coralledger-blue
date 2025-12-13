namespace CoralLedger.Blue.Domain.Enums;

/// <summary>
/// Geometry resolution levels for map display performance optimization.
/// 4-tier system optimized for different map zoom levels and use cases.
/// </summary>
public enum GeometryResolution
{
    /// <summary>
    /// Full original geometry with no simplification.
    /// Use for: Data exports, precise calculations, compliance reports.
    /// Tolerance: 0 (original)
    /// </summary>
    Full,

    /// <summary>
    /// Detail tier with minimal simplification (~0.0001° tolerance, ~10m at 25°N).
    /// Use for: Close zoom views, boundary inspection, detailed editing.
    /// Tolerance: 0.0001°
    /// </summary>
    Detail,

    /// <summary>
    /// Medium simplification (~0.001° tolerance, ~100m at 25°N latitude).
    /// Use for: Default map view, most interactive scenarios.
    /// Tolerance: 0.001°
    /// </summary>
    Medium,

    /// <summary>
    /// Low resolution (~0.01° tolerance, ~1km at 25°N latitude).
    /// Use for: Overview maps, thumbnails, low-bandwidth scenarios.
    /// Tolerance: 0.01°
    /// </summary>
    Low
}
