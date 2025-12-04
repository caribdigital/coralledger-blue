namespace CoralLedger.Application.Common.Interfaces;

/// <summary>
/// Distributed cache service for performance optimization
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Get a cached value by key
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class;

    /// <summary>
    /// Set a cached value with optional expiration
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default) where T : class;

    /// <summary>
    /// Remove a cached value by key
    /// </summary>
    Task RemoveAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Remove all cached values matching a pattern
    /// </summary>
    Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default);

    /// <summary>
    /// Get or set a cached value using a factory function
    /// </summary>
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken ct = default) where T : class;
}

/// <summary>
/// Cache key constants for consistent naming
/// </summary>
public static class CacheKeys
{
    public const string MpaList = "mpas:list";
    public const string MpaGeoJson = "mpas:geojson";
    public const string MpaStats = "mpas:stats";
    public const string MpaDetail = "mpas:detail:{0}";

    public const string BleachingAlerts = "bleaching:alerts";
    public const string BleachingLatest = "bleaching:latest:{0}";

    public const string VesselPositions = "vessels:positions";
    public const string VesselEvents = "vessels:events";

    public const string AlertsActive = "alerts:active";
    public const string AlertRules = "alerts:rules";

    public const string DashboardStats = "admin:dashboard";

    public static string ForMpa(Guid id) => string.Format(MpaDetail, id);
    public static string ForBleaching(Guid mpaId) => string.Format(BleachingLatest, mpaId);
}

/// <summary>
/// Cache duration presets
/// </summary>
public static class CacheDurations
{
    public static readonly TimeSpan VeryShort = TimeSpan.FromSeconds(30);
    public static readonly TimeSpan Short = TimeSpan.FromMinutes(1);
    public static readonly TimeSpan Medium = TimeSpan.FromMinutes(5);
    public static readonly TimeSpan Long = TimeSpan.FromMinutes(30);
    public static readonly TimeSpan VeryLong = TimeSpan.FromHours(1);
    public static readonly TimeSpan Day = TimeSpan.FromDays(1);
}
