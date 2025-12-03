namespace CoralLedger.Domain.Enums;

/// <summary>
/// Types of vessel events tracked by Global Fishing Watch
/// </summary>
public enum VesselEventType
{
    Unknown = 0,
    Fishing = 1,          // Apparent fishing activity
    Encounter = 2,        // Meeting between vessels at sea
    Loitering = 3,        // Carrier vessel waiting (potential transshipment)
    PortVisit = 4,        // Port entry/exit
    AisDisabling = 5,     // Gap in AIS transmission
    Transshipment = 6     // Fish transfer between vessels
}
