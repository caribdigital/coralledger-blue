namespace CoralLedger.Blue.Domain.Enums;

/// <summary>
/// Types of alerts that can be generated
/// </summary>
public enum AlertType
{
    /// <summary>
    /// Coral bleaching threshold exceeded
    /// </summary>
    Bleaching = 1,

    /// <summary>
    /// Unusual fishing activity detected
    /// </summary>
    FishingActivity = 2,

    /// <summary>
    /// Vessel detected inside protected area
    /// </summary>
    VesselInMPA = 3,

    /// <summary>
    /// Vessel AIS signal lost
    /// </summary>
    VesselDarkEvent = 4,

    /// <summary>
    /// New citizen observation submitted
    /// </summary>
    CitizenObservation = 5,

    /// <summary>
    /// Sea surface temperature anomaly
    /// </summary>
    TemperatureAnomaly = 6,

    /// <summary>
    /// High Degree Heating Week value
    /// </summary>
    DegreeHeatingWeek = 7,

    /// <summary>
    /// System/infrastructure alert
    /// </summary>
    System = 99
}
