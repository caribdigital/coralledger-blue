using CoralLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoralLedger.Infrastructure.Data.Configurations;

public class VesselConfiguration : IEntityTypeConfiguration<Vessel>
{
    public void Configure(EntityTypeBuilder<Vessel> builder)
    {
        builder.ToTable("vessels");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Mmsi)
            .HasMaxLength(9);

        builder.Property(e => e.Imo)
            .HasMaxLength(7);

        builder.Property(e => e.CallSign)
            .HasMaxLength(20);

        builder.Property(e => e.GfwVesselId)
            .HasMaxLength(100);

        builder.Property(e => e.Flag)
            .HasMaxLength(3);  // ISO 3166-1 alpha-3

        builder.Property(e => e.VesselType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.GearType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.LengthMeters)
            .HasPrecision(10, 2);

        builder.Property(e => e.TonnageGt)
            .HasPrecision(12, 2);

        // Indexes
        builder.HasIndex(e => e.Mmsi);
        builder.HasIndex(e => e.Imo);
        builder.HasIndex(e => e.GfwVesselId);
        builder.HasIndex(e => e.Name);
        builder.HasIndex(e => e.Flag);
        builder.HasIndex(e => e.VesselType);
        builder.HasIndex(e => e.IsActive);

        // Relationships
        builder.HasMany(e => e.Positions)
            .WithOne(p => p.Vessel)
            .HasForeignKey(p => p.VesselId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Events)
            .WithOne(ev => ev.Vessel)
            .HasForeignKey(ev => ev.VesselId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
