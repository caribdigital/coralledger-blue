using CoralLedger.Blue.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoralLedger.Blue.Infrastructure.Data.Configurations;

public class VesselEventConfiguration : IEntityTypeConfiguration<VesselEvent>
{
    public void Configure(EntityTypeBuilder<VesselEvent> builder)
    {
        builder.ToTable("vessel_events");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.GfwEventId)
            .HasMaxLength(100);

        builder.Property(e => e.EventType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        // Spatial column
        builder.Property(e => e.Location)
            .HasColumnType("geometry(Point, 4326)")
            .IsRequired();

        builder.Property(e => e.StartTime)
            .IsRequired();

        builder.Property(e => e.DurationHours)
            .HasPrecision(10, 2);

        builder.Property(e => e.DistanceKm)
            .HasPrecision(10, 3);

        builder.Property(e => e.PortName)
            .HasMaxLength(200);

        builder.Property(e => e.EncounterVesselId)
            .HasMaxLength(100);

        // Spatial index
        builder.HasIndex(e => e.Location)
            .HasMethod("GIST");

        // Regular indexes
        builder.HasIndex(e => e.GfwEventId);
        builder.HasIndex(e => e.EventType);
        builder.HasIndex(e => e.StartTime);
        builder.HasIndex(e => e.VesselId);
        builder.HasIndex(e => e.IsInMpa);
        builder.HasIndex(e => new { e.VesselId, e.EventType, e.StartTime });

        // Relationships
        builder.HasOne(e => e.MarineProtectedArea)
            .WithMany()
            .HasForeignKey(e => e.MarineProtectedAreaId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
