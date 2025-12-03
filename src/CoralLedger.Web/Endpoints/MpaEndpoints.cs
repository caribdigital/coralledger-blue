using CoralLedger.Application.Features.MarineProtectedAreas.Commands.SyncFromWdpa;
using CoralLedger.Application.Features.MarineProtectedAreas.Queries.GetAllMpas;
using CoralLedger.Application.Features.MarineProtectedAreas.Queries.GetMpaById;
using CoralLedger.Application.Features.MarineProtectedAreas.Queries.GetMpasGeoJson;
using CoralLedger.Domain.Enums;
using MediatR;

namespace CoralLedger.Web.Endpoints;

public static class MpaEndpoints
{
    public static IEndpointRouteBuilder MapMpaEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/mpas")
            .WithTags("Marine Protected Areas");

        // GET /api/mpas - Get all MPAs (summary)
        group.MapGet("/", async (IMediator mediator, CancellationToken ct) =>
        {
            var mpas = await mediator.Send(new GetAllMpasQuery(), ct);
            return Results.Ok(mpas);
        })
        .WithName("GetAllMpas")
        .WithDescription("Get all Marine Protected Areas with summary information")
        .Produces<IReadOnlyList<CoralLedger.Application.Features.MarineProtectedAreas.DTOs.MpaSummaryDto>>();

        // GET /api/mpas/geojson - Get all MPAs as GeoJSON FeatureCollection
        // ?resolution=full|medium|low (default: medium)
        group.MapGet("/geojson", async (
            string? resolution,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var res = ParseResolution(resolution);
            var geoJson = await mediator.Send(new GetMpasGeoJsonQuery(res), ct);
            return Results.Ok(geoJson);
        })
        .WithName("GetMpasGeoJson")
        .WithDescription("Get all Marine Protected Areas as GeoJSON FeatureCollection for map display. " +
            "Use ?resolution=full|medium|low to control geometry simplification (default: medium)")
        .Produces<MpaGeoJsonCollection>();

        // GET /api/mpas/{id} - Get specific MPA by ID
        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var mpa = await mediator.Send(new GetMpaByIdQuery(id), ct);
            return mpa is null ? Results.NotFound() : Results.Ok(mpa);
        })
        .WithName("GetMpaById")
        .WithDescription("Get detailed information about a specific Marine Protected Area")
        .Produces<CoralLedger.Application.Features.MarineProtectedAreas.DTOs.MpaDetailDto>()
        .Produces(StatusCodes.Status404NotFound);

        // POST /api/mpas/{id}/sync-wdpa - Sync MPA boundary from WDPA
        group.MapPost("/{id:guid}/sync-wdpa", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new SyncMpaFromWdpaCommand(id), ct);
            return result.Success
                ? Results.Ok(result)
                : Results.BadRequest(result);
        })
        .WithName("SyncMpaFromWdpa")
        .WithDescription("Sync MPA boundary geometry from Protected Planet WDPA API")
        .Produces<SyncResult>()
        .Produces<SyncResult>(StatusCodes.Status400BadRequest);

        return endpoints;
    }

    private static GeometryResolution ParseResolution(string? resolution) =>
        resolution?.ToLowerInvariant() switch
        {
            "full" => GeometryResolution.Full,
            "low" => GeometryResolution.Low,
            _ => GeometryResolution.Medium // Default
        };
}
