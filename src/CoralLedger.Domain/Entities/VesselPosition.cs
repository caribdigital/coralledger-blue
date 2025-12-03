using CoralLedger.Domain.Common;
using NetTopologySuite.Geometries;

namespace CoralLedger.Domain.Entities;

/// <summary>
/// Represents a vessel's position at a specific point in time
/// Typically sourced from AIS data via Global Fishing Watch
/// </summary>
public class VesselPosition : BaseEntity, IAuditableEntity
{
    public Point Location { get; private set; } = null!;
    public DateTime Timestamp { get; private set; }
    public double? SpeedKnots { get; private set; }
    public double? CourseOverGround { get; private set; }  // Degrees (0-360)
    public double? Heading { get; private set; }           // Degrees (0-360)
    public bool? IsInMpa { get; private set; }             // Was vessel inside any MPA at this position
    public double? DistanceFromShoreKm { get; private set; }

    // Foreign keys
    public Guid VesselId { get; private set; }
    public Vessel Vessel { get; private set; } = null!;

    public Guid? MarineProtectedAreaId { get; private set; }  // MPA if inside one
    public MarineProtectedArea? MarineProtectedArea { get; private set; }

    // Audit fields
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }

    private VesselPosition() { }

    public static VesselPosition Create(
        Guid vesselId,
        Point location,
        DateTime timestamp,
        double? speedKnots = null,
        double? courseOverGround = null,
        double? heading = null)
    {
        return new VesselPosition
        {
            Id = Guid.NewGuid(),
            VesselId = vesselId,
            Location = location,
            Timestamp = timestamp,
            SpeedKnots = speedKnots,
            CourseOverGround = courseOverGround,
            Heading = heading,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void SetMpaContext(bool isInMpa, Guid? mpaId = null)
    {
        IsInMpa = isInMpa;
        MarineProtectedAreaId = mpaId;
        ModifiedAt = DateTime.UtcNow;
    }

    public void SetDistanceFromShore(double distanceKm)
    {
        DistanceFromShoreKm = distanceKm;
        ModifiedAt = DateTime.UtcNow;
    }
}
