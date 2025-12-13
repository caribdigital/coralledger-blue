using CoralLedger.Blue.Domain.Common;
using CoralLedger.Blue.Domain.Enums;

namespace CoralLedger.Blue.Domain.Entities;

/// <summary>
/// Represents a vessel tracked via AIS (Automatic Identification System)
/// Data sourced from Global Fishing Watch API
/// </summary>
public class Vessel : BaseEntity, IAggregateRoot, IAuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Mmsi { get; private set; }         // Maritime Mobile Service Identity (9-digit)
    public string? Imo { get; private set; }          // IMO number (7-digit)
    public string? CallSign { get; private set; }
    public string? GfwVesselId { get; private set; }  // Global Fishing Watch internal vessel ID
    public string? Flag { get; private set; }         // ISO 3166-1 alpha-3 country code
    public VesselType VesselType { get; private set; }
    public GearType? GearType { get; private set; }
    public double? LengthMeters { get; private set; }
    public double? TonnageGt { get; private set; }    // Gross tonnage
    public int? YearBuilt { get; private set; }
    public DateTime? LastPositionTime { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Audit fields
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }

    // Navigation properties
    public ICollection<VesselPosition> Positions { get; private set; } = new List<VesselPosition>();
    public ICollection<VesselEvent> Events { get; private set; } = new List<VesselEvent>();

    private Vessel() { }

    public static Vessel Create(
        string name,
        string? mmsi = null,
        string? imo = null,
        string? gfwVesselId = null,
        string? flag = null,
        VesselType vesselType = VesselType.Unknown,
        GearType? gearType = null)
    {
        return new Vessel
        {
            Id = Guid.NewGuid(),
            Name = name,
            Mmsi = mmsi,
            Imo = imo,
            GfwVesselId = gfwVesselId,
            Flag = flag,
            VesselType = vesselType,
            GearType = gearType,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateFromApi(
        string name,
        string? callSign,
        string? flag,
        VesselType vesselType,
        GearType? gearType,
        double? lengthMeters,
        double? tonnageGt,
        int? yearBuilt)
    {
        Name = name;
        CallSign = callSign;
        Flag = flag;
        VesselType = vesselType;
        GearType = gearType;
        LengthMeters = lengthMeters;
        TonnageGt = tonnageGt;
        YearBuilt = yearBuilt;
        ModifiedAt = DateTime.UtcNow;
    }

    public void UpdateLastPosition(DateTime positionTime)
    {
        LastPositionTime = positionTime;
        ModifiedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        ModifiedAt = DateTime.UtcNow;
    }
}
