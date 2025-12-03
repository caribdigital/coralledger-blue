using CoralLedger.Application.Common.Interfaces;
using CoralLedger.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CoralLedger.Application.Features.MarineProtectedAreas.Queries.GetMpasGeoJson;

/// <summary>
/// Query to get all MPAs as GeoJSON FeatureCollection with configurable geometry resolution
/// </summary>
/// <param name="Resolution">Geometry resolution level (default: Medium for map performance)</param>
public record GetMpasGeoJsonQuery(GeometryResolution Resolution = GeometryResolution.Medium) : IRequest<MpaGeoJsonCollection>;

public class MpaGeoJsonCollection
{
    public string Type { get; init; } = "FeatureCollection";
    public List<MpaGeoJsonFeature> Features { get; init; } = new();
}

public class MpaGeoJsonFeature
{
    public string Type { get; init; } = "Feature";
    public string Id { get; init; } = "";
    public JsonNode? Geometry { get; init; }
    public MpaGeoJsonProperties Properties { get; init; } = new();
}

public class MpaGeoJsonProperties
{
    public string Name { get; init; } = "";
    public string ProtectionLevel { get; init; } = "";
    public string IslandGroup { get; init; } = "";
    public double AreaSquareKm { get; init; }
    public string Status { get; init; } = "";
    public double CentroidLongitude { get; init; }
    public double CentroidLatitude { get; init; }
}

public class GetMpasGeoJsonQueryHandler : IRequestHandler<GetMpasGeoJsonQuery, MpaGeoJsonCollection>
{
    private readonly IMarineDbContext _context;

    public GetMpasGeoJsonQueryHandler(IMarineDbContext context)
    {
        _context = context;
    }

    public async Task<MpaGeoJsonCollection> Handle(
        GetMpasGeoJsonQuery request,
        CancellationToken cancellationToken)
    {
        var mpas = await _context.MarineProtectedAreas
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var geoJsonWriter = new GeoJsonWriter();
        var features = new List<MpaGeoJsonFeature>();

        foreach (var mpa in mpas)
        {
            // Select appropriate geometry based on requested resolution
            var geometry = request.Resolution switch
            {
                GeometryResolution.Full => mpa.Boundary,
                GeometryResolution.Low => mpa.BoundarySimplifiedLow ?? mpa.Boundary,
                _ => mpa.BoundarySimplifiedMedium ?? mpa.Boundary // Medium is default
            };

            var geometryJson = geoJsonWriter.Write(geometry);
            var geometryNode = JsonNode.Parse(geometryJson);

            features.Add(new MpaGeoJsonFeature
            {
                Id = mpa.Id.ToString(),
                Geometry = geometryNode,
                Properties = new MpaGeoJsonProperties
                {
                    Name = mpa.Name,
                    ProtectionLevel = mpa.ProtectionLevel.ToString(),
                    IslandGroup = mpa.IslandGroup.ToString(),
                    AreaSquareKm = mpa.AreaSquareKm,
                    Status = mpa.Status.ToString(),
                    CentroidLongitude = mpa.Centroid.X,
                    CentroidLatitude = mpa.Centroid.Y
                }
            });
        }

        return new MpaGeoJsonCollection { Features = features };
    }
}
