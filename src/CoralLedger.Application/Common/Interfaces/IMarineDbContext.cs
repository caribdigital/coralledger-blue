using CoralLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoralLedger.Application.Common.Interfaces;

public interface IMarineDbContext
{
    DbSet<MarineProtectedArea> MarineProtectedAreas { get; }
    DbSet<Reef> Reefs { get; }
    DbSet<Vessel> Vessels { get; }
    DbSet<VesselPosition> VesselPositions { get; }
    DbSet<VesselEvent> VesselEvents { get; }
    DbSet<BleachingAlert> BleachingAlerts { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
