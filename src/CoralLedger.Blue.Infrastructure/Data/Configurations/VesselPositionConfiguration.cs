using CoralLedger.Blue.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoralLedger.Blue.Infrastructure.Data.Configurations;

public class VesselPositionConfiguration : IEntityTypeConfiguration<VesselPosition>
{
    public void Configure(EntityTypeBuilder<VesselPosition> builder)
    {
        builder.ToTable("vessel_positions");

        builder.HasKey(e => e.Id);

        // Spatial column
        builder.Property(e => e.Location)
            .HasColumnType("geometry(Point, 4326)")
            .IsRequired();

        builder.Property(e => e.Timestamp)
            .IsRequired();

        builder.Property(e => e.SpeedKnots)
            .HasPrecision(6, 2);

        builder.Property(e => e.CourseOverGround)
            .HasPrecision(6, 2);

        builder.Property(e => e.Heading)
            .HasPrecision(6, 2);

        builder.Property(e => e.DistanceFromShoreKm)
            .HasPrecision(10, 3);

        // Spatial index for efficient querying
        builder.HasIndex(e => e.Location)
            .HasMethod("GIST");

        // Regular indexes
        builder.HasIndex(e => e.Timestamp);
        builder.HasIndex(e => e.VesselId);
        builder.HasIndex(e => e.IsInMpa);
        builder.HasIndex(e => new { e.VesselId, e.Timestamp });

        // Relationships
        builder.HasOne(e => e.MarineProtectedArea)
            .WithMany()
            .HasForeignKey(e => e.MarineProtectedAreaId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
