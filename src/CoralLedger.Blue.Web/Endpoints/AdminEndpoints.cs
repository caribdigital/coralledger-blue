using CoralLedger.Blue.Application.Common.Interfaces;
using CoralLedger.Blue.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CoralLedger.Blue.Web.Endpoints;

public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/admin")
            .WithTags("Administration");

        // GET /api/admin/dashboard - Admin dashboard stats
        group.MapGet("/dashboard", async (
            IMarineDbContext context,
            CancellationToken ct = default) =>
        {
            var mpaCount = await context.MarineProtectedAreas.CountAsync(ct);
            var reefCount = await context.Reefs.CountAsync(ct);
            var vesselCount = await context.Vessels.CountAsync(ct);
            var observationCount = await context.CitizenObservations.CountAsync(ct);
            var alertRuleCount = await context.AlertRules.CountAsync(ct);
            var activeAlertCount = await context.Alerts.CountAsync(a => !a.IsAcknowledged, ct);

            var recentBleaching = await context.BleachingAlerts
                .Where(b => b.Date >= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7)))
                .CountAsync(ct);

            var recentEvents = await context.VesselEvents
                .Where(e => e.StartTime >= DateTime.UtcNow.AddDays(-7))
                .CountAsync(ct);

            var pendingObservations = await context.CitizenObservations
                .CountAsync(o => o.Status == ObservationStatus.Pending, ct);

            return Results.Ok(new
            {
                counts = new
                {
                    mpas = mpaCount,
                    reefs = reefCount,
                    vessels = vesselCount,
                    observations = observationCount,
                    alertRules = alertRuleCount,
                    activeAlerts = activeAlertCount
                },
                recent = new
                {
                    bleachingAlerts = recentBleaching,
                    vesselEvents = recentEvents,
                    pendingObservations = pendingObservations
                },
                lastUpdated = DateTime.UtcNow
            });
        })
        .WithName("GetAdminDashboard")
        .Produces<object>();

        // GET /api/admin/observations/pending - Get pending observations for moderation
        group.MapGet("/observations/pending", async (
            IMarineDbContext context,
            int page = 1,
            int pageSize = 20,
            CancellationToken ct = default) =>
        {
            var query = context.CitizenObservations
                .Include(o => o.Photos)
                .Where(o => o.Status == ObservationStatus.Pending)
                .OrderByDescending(o => o.CreatedAt);

            var total = await query.CountAsync(ct);
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new
                {
                    o.Id,
                    o.Title,
                    o.Description,
                    Type = o.Type.ToString(),
                    o.Severity,
                    o.ObservationTime,
                    o.CreatedAt,
                    o.CitizenEmail,
                    o.CitizenName,
                    PhotoCount = o.Photos.Count,
                    Location = o.Location != null ? new { Lon = o.Location.X, Lat = o.Location.Y } : null
                })
                .ToListAsync(ct);

            return Results.Ok(new
            {
                items,
                total,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(total / (double)pageSize)
            });
        })
        .WithName("GetPendingObservations")
        .Produces<object>();

        // POST /api/admin/observations/{id}/approve - Approve an observation
        group.MapPost("/observations/{id:guid}/approve", async (
            Guid id,
            ApproveRequest? request,
            IMarineDbContext context,
            CancellationToken ct = default) =>
        {
            var observation = await context.CitizenObservations.FindAsync(new object[] { id }, ct);
            if (observation == null)
                return Results.NotFound();

            observation.Approve(request?.Notes);
            await context.SaveChangesAsync(ct);

            return Results.Ok(new { observation.Id, Status = "Approved" });
        })
        .WithName("ApproveObservation")
        .Produces<object>()
        .Produces(StatusCodes.Status404NotFound);

        // POST /api/admin/observations/{id}/reject - Reject an observation
        group.MapPost("/observations/{id:guid}/reject", async (
            Guid id,
            RejectRequest request,
            IMarineDbContext context,
            CancellationToken ct = default) =>
        {
            var observation = await context.CitizenObservations.FindAsync(new object[] { id }, ct);
            if (observation == null)
                return Results.NotFound();

            if (string.IsNullOrWhiteSpace(request.Reason))
                return Results.BadRequest("Rejection reason is required");

            observation.Reject(request.Reason);
            await context.SaveChangesAsync(ct);

            return Results.Ok(new { observation.Id, Status = "Rejected" });
        })
        .WithName("RejectObservation")
        .Produces<object>()
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        // GET /api/admin/system/health - System health check
        group.MapGet("/system/health", async (
            IMarineDbContext context,
            CancellationToken ct = default) =>
        {
            var dbHealthy = false;
            try
            {
                await context.Database.ExecuteSqlRawAsync("SELECT 1", ct);
                dbHealthy = true;
            }
            catch { }

            return Results.Ok(new
            {
                status = dbHealthy ? "Healthy" : "Unhealthy",
                database = dbHealthy ? "Connected" : "Disconnected",
                timestamp = DateTime.UtcNow
            });
        })
        .WithName("GetSystemHealth")
        .Produces<object>();

        // GET /api/admin/system/config - Get system configuration
        group.MapGet("/system/config", (IConfiguration configuration) =>
        {
            return Results.Ok(new
            {
                features = new
                {
                    aiEnabled = !string.IsNullOrEmpty(configuration["MarineAI:ApiKey"]),
                    aisEnabled = configuration.GetValue<bool>("AIS:Enabled"),
                    blobStorageConfigured = !string.IsNullOrEmpty(configuration["BlobStorage:ConnectionString"]),
                    gfwConfigured = !string.IsNullOrEmpty(configuration["GlobalFishingWatch:ApiKey"])
                },
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
            });
        })
        .WithName("GetSystemConfig")
        .Produces<object>();

        // GET /api/admin/jobs - Get background job status
        group.MapGet("/jobs", async (
            IMarineDbContext context,
            CancellationToken ct = default) =>
        {
            // Get latest sync timestamps from data
            var latestBleaching = await context.BleachingAlerts
                .OrderByDescending(b => b.Date)
                .Select(b => b.Date)
                .FirstOrDefaultAsync(ct);

            var latestVesselEvent = await context.VesselEvents
                .OrderByDescending(e => e.CreatedAt)
                .Select(e => e.CreatedAt)
                .FirstOrDefaultAsync(ct);

            return Results.Ok(new
            {
                jobs = new[]
                {
                    new { name = "BleachingDataSync", schedule = "Daily 6:00 UTC", lastData = latestBleaching.ToString("yyyy-MM-dd") },
                    new { name = "VesselEventSync", schedule = "Every 6 hours", lastData = latestVesselEvent.ToString("o") }
                }
            });
        })
        .WithName("GetAdminJobs")
        .Produces<object>();

        // GET /api/admin/data/summary - Data summary statistics
        group.MapGet("/data/summary", async (
            IMarineDbContext context,
            CancellationToken ct = default) =>
        {
            var mpasByIsland = await context.MarineProtectedAreas
                .GroupBy(m => m.IslandGroup)
                .Select(g => new { IslandGroup = g.Key.ToString(), Count = g.Count(), TotalAreaKm2 = g.Sum(m => m.AreaSquareKm) })
                .ToListAsync(ct);

            var mpasByProtection = await context.MarineProtectedAreas
                .GroupBy(m => m.ProtectionLevel)
                .Select(g => new { Level = g.Key.ToString(), Count = g.Count() })
                .ToListAsync(ct);

            var observationsByType = await context.CitizenObservations
                .GroupBy(o => o.Type)
                .Select(g => new { Type = g.Key.ToString(), Count = g.Count() })
                .ToListAsync(ct);

            var observationsByStatus = await context.CitizenObservations
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
                .ToListAsync(ct);

            return Results.Ok(new
            {
                mpas = new { byIslandGroup = mpasByIsland, byProtectionLevel = mpasByProtection },
                observations = new { byType = observationsByType, byStatus = observationsByStatus }
            });
        })
        .WithName("GetDataSummary")
        .Produces<object>();

        return endpoints;
    }
}

public record ApproveRequest(string? Notes);
public record RejectRequest(string Reason);
