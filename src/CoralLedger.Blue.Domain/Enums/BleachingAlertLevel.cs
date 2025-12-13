namespace CoralLedger.Blue.Domain.Enums;

/// <summary>
/// NOAA Coral Reef Watch Bleaching Alert Levels (Version 3.1)
/// Based on Degree Heating Week (DHW) thresholds
/// </summary>
public enum BleachingAlertLevel
{
    NoStress = 0,           // DHW = 0
    BleachingWatch = 1,     // 0 < DHW < 4
    BleachingWarning = 2,   // 4 <= DHW < 8
    AlertLevel1 = 3,        // DHW >= 4 with current HotSpot >= 1
    AlertLevel2 = 4,        // DHW >= 8
    AlertLevel3 = 5,        // DHW >= 12 (NEW in v3.1)
    AlertLevel4 = 6,        // DHW >= 16 (NEW in v3.1)
    AlertLevel5 = 7         // DHW >= 20 (NEW in v3.1)
}
