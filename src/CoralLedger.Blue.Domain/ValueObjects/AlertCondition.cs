namespace CoralLedger.Blue.Domain.ValueObjects;

/// <summary>
/// Base class for alert conditions
/// </summary>
public abstract record AlertCondition
{
    public abstract string Type { get; }
}

/// <summary>
/// Condition for bleaching alerts
/// </summary>
public record BleachingCondition : AlertCondition
{
    public override string Type => "Bleaching";

    /// <summary>
    /// Minimum alert level to trigger (0=NoStress to 5=AlertLevel5)
    /// </summary>
    public int MinAlertLevel { get; init; } = 2;

    /// <summary>
    /// Minimum DHW value to trigger
    /// </summary>
    public double? MinDegreeHeatingWeek { get; init; }

    /// <summary>
    /// Minimum SST anomaly to trigger
    /// </summary>
    public double? MinSstAnomaly { get; init; }
}

/// <summary>
/// Condition for fishing activity alerts
/// </summary>
public record FishingActivityCondition : AlertCondition
{
    public override string Type => "FishingActivity";

    /// <summary>
    /// Minimum number of fishing events in time window
    /// </summary>
    public int MinEventCount { get; init; } = 5;

    /// <summary>
    /// Time window for counting events (hours)
    /// </summary>
    public int TimeWindowHours { get; init; } = 24;

    /// <summary>
    /// Only alert on fishing inside MPAs
    /// </summary>
    public bool OnlyInsideMpa { get; init; } = true;
}

/// <summary>
/// Condition for vessel in MPA alerts
/// </summary>
public record VesselInMpaCondition : AlertCondition
{
    public override string Type => "VesselInMPA";

    /// <summary>
    /// Duration vessel must be in MPA before alert (minutes)
    /// </summary>
    public int MinDurationMinutes { get; init; } = 30;

    /// <summary>
    /// Only alert on fishing vessels
    /// </summary>
    public bool OnlyFishingVessels { get; init; } = true;

    /// <summary>
    /// Only alert in NoTake zones
    /// </summary>
    public bool OnlyNoTakeZones { get; init; } = false;
}

/// <summary>
/// Condition for vessel dark events (AIS signal loss)
/// </summary>
public record VesselDarkCondition : AlertCondition
{
    public override string Type => "VesselDarkEvent";

    /// <summary>
    /// Duration of signal loss before alert (minutes)
    /// </summary>
    public int MinDarkDurationMinutes { get; init; } = 60;

    /// <summary>
    /// Only alert if vessel was near MPA when signal lost
    /// </summary>
    public bool OnlyNearMpa { get; init; } = true;

    /// <summary>
    /// Distance from MPA to consider "near" (km)
    /// </summary>
    public double NearMpaDistanceKm { get; init; } = 10;
}

/// <summary>
/// Condition for temperature anomaly alerts
/// </summary>
public record TemperatureCondition : AlertCondition
{
    public override string Type => "TemperatureAnomaly";

    /// <summary>
    /// SST threshold in Celsius
    /// </summary>
    public double? MaxSst { get; init; }

    /// <summary>
    /// SST anomaly threshold (degrees above normal)
    /// </summary>
    public double? MaxSstAnomaly { get; init; } = 1.0;
}

/// <summary>
/// Condition for citizen observation alerts
/// </summary>
public record CitizenObservationCondition : AlertCondition
{
    public override string Type => "CitizenObservation";

    /// <summary>
    /// Only alert on observations with health status below this
    /// </summary>
    public int? MaxHealthStatus { get; init; } = 2;

    /// <summary>
    /// Keywords to match in description (comma-separated)
    /// </summary>
    public string? Keywords { get; init; }
}
