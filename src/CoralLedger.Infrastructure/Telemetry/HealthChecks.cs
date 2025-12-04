using CoralLedger.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace CoralLedger.Infrastructure.Telemetry;

/// <summary>
/// Health check for the Marine PostgreSQL database
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly IMarineDbContext _context;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(IMarineDbContext context, ILogger<DatabaseHealthCheck> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check database connectivity and basic query
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);

            if (!canConnect)
            {
                return HealthCheckResult.Unhealthy("Cannot connect to database");
            }

            // Check if we can query data
            var mpaCount = await _context.MarineProtectedAreas.CountAsync(cancellationToken);

            return HealthCheckResult.Healthy($"Database is healthy. {mpaCount} MPAs in database.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return HealthCheckResult.Unhealthy("Database health check failed", ex);
        }
    }
}

/// <summary>
/// Health check for NOAA Coral Reef Watch API
/// </summary>
public class NoaaHealthCheck : IHealthCheck
{
    private readonly ICoralReefWatchClient _client;
    private readonly ILogger<NoaaHealthCheck> _logger;

    public NoaaHealthCheck(ICoralReefWatchClient client, ILogger<NoaaHealthCheck> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Try to fetch current data for a test location (Nassau)
            // Parameters: longitude, latitude, date
            var data = await _client.GetBleachingDataAsync(-77.35, 25.05, DateOnly.FromDateTime(DateTime.UtcNow), cancellationToken);

            if (data is null)
            {
                return HealthCheckResult.Degraded("NOAA API returned no data");
            }

            return HealthCheckResult.Healthy("NOAA Coral Reef Watch API is healthy");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "NOAA API health check failed");
            return HealthCheckResult.Degraded("NOAA API is not responding", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "NOAA API health check error");
            return HealthCheckResult.Unhealthy("NOAA API health check failed", ex);
        }
    }
}

/// <summary>
/// Health check for Global Fishing Watch API
/// </summary>
public class GfwHealthCheck : IHealthCheck
{
    private readonly IGlobalFishingWatchClient _client;
    private readonly ILogger<GfwHealthCheck> _logger;

    public GfwHealthCheck(IGlobalFishingWatchClient client, ILogger<GfwHealthCheck> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if API is accessible (this will fail if not configured, which is acceptable)
            var isConfigured = _client.IsConfigured;

            if (!isConfigured)
            {
                return HealthCheckResult.Degraded("Global Fishing Watch API is not configured");
            }

            return HealthCheckResult.Healthy("Global Fishing Watch API is configured");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GFW API health check error");
            return HealthCheckResult.Unhealthy("GFW API health check failed", ex);
        }
    }
}

/// <summary>
/// Health check for Azure Blob Storage
/// </summary>
public class BlobStorageHealthCheck : IHealthCheck
{
    private readonly IBlobStorageService _blobService;
    private readonly ILogger<BlobStorageHealthCheck> _logger;

    public BlobStorageHealthCheck(IBlobStorageService blobService, ILogger<BlobStorageHealthCheck> logger)
    {
        _blobService = blobService;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var isConfigured = _blobService.IsConfigured;

            if (!isConfigured)
            {
                return HealthCheckResult.Degraded("Azure Blob Storage is not configured");
            }

            return HealthCheckResult.Healthy("Azure Blob Storage is configured");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Blob storage health check error");
            return HealthCheckResult.Unhealthy("Blob storage health check failed", ex);
        }
    }
}

/// <summary>
/// Health check for background job scheduler
/// </summary>
public class QuartzHealthCheck : IHealthCheck
{
    private readonly Quartz.ISchedulerFactory _schedulerFactory;
    private readonly ILogger<QuartzHealthCheck> _logger;

    public QuartzHealthCheck(Quartz.ISchedulerFactory schedulerFactory, ILogger<QuartzHealthCheck> logger)
    {
        _schedulerFactory = schedulerFactory;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);

            if (!scheduler.IsStarted)
            {
                return HealthCheckResult.Degraded("Quartz scheduler is not started");
            }

            if (scheduler.InStandbyMode)
            {
                return HealthCheckResult.Degraded("Quartz scheduler is in standby mode");
            }

            var runningJobs = await scheduler.GetCurrentlyExecutingJobs(cancellationToken);

            return HealthCheckResult.Healthy($"Quartz scheduler is running. {runningJobs.Count} jobs executing.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Quartz health check error");
            return HealthCheckResult.Unhealthy("Quartz health check failed", ex);
        }
    }
}
