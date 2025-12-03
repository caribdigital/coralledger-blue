using CoralLedger.Domain.Common;
using CoralLedger.Domain.Enums;
using NetTopologySuite.Geometries;

namespace CoralLedger.Domain.Entities;

/// <summary>
/// Represents a significant vessel event (fishing, encounter, port visit, etc.)
/// Data sourced from Global Fishing Watch Events API
/// </summary>
public class VesselEvent : BaseEntity, IAuditableEntity
{
    public string? GfwEventId { get; private set; }  // Global Fishing Watch event ID
    public VesselEventType EventType { get; private set; }
    public Point Location { get; private set; } = null!;
    public DateTime StartTime { get; private set; }
    public DateTime? EndTime { get; private set; }
    public double? DurationHours { get; private set; }
    public double? DistanceKm { get; private set; }          // For fishing: distance traveled while fishing
    public string? PortName { get; private set; }            // For port visits
    public string? EncounterVesselId { get; private set; }   // For encounters: ID of other vessel
    public bool? IsInMpa { get; private set; }               // Did event occur within an MPA

    // Foreign keys
    public Guid VesselId { get; private set; }
    public Vessel Vessel { get; private set; } = null!;

    public Guid? MarineProtectedAreaId { get; private set; }
    public MarineProtectedArea? MarineProtectedArea { get; private set; }

    // Audit fields
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }

    private VesselEvent() { }

    public static VesselEvent CreateFishingEvent(
        Guid vesselId,
        Point location,
        DateTime startTime,
        DateTime? endTime,
        double? durationHours,
        double? distanceKm,
        string? gfwEventId = null)
    {
        return new VesselEvent
        {
            Id = Guid.NewGuid(),
            VesselId = vesselId,
            EventType = VesselEventType.Fishing,
            Location = location,
            StartTime = startTime,
            EndTime = endTime,
            DurationHours = durationHours,
            DistanceKm = distanceKm,
            GfwEventId = gfwEventId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static VesselEvent CreatePortVisit(
        Guid vesselId,
        Point location,
        DateTime startTime,
        DateTime? endTime,
        string portName,
        string? gfwEventId = null)
    {
        return new VesselEvent
        {
            Id = Guid.NewGuid(),
            VesselId = vesselId,
            EventType = VesselEventType.PortVisit,
            Location = location,
            StartTime = startTime,
            EndTime = endTime,
            PortName = portName,
            GfwEventId = gfwEventId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static VesselEvent CreateEncounter(
        Guid vesselId,
        Point location,
        DateTime startTime,
        DateTime? endTime,
        string encounterVesselId,
        string? gfwEventId = null)
    {
        return new VesselEvent
        {
            Id = Guid.NewGuid(),
            VesselId = vesselId,
            EventType = VesselEventType.Encounter,
            Location = location,
            StartTime = startTime,
            EndTime = endTime,
            EncounterVesselId = encounterVesselId,
            GfwEventId = gfwEventId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void SetMpaContext(bool isInMpa, Guid? mpaId = null)
    {
        IsInMpa = isInMpa;
        MarineProtectedAreaId = mpaId;
        ModifiedAt = DateTime.UtcNow;
    }
}
