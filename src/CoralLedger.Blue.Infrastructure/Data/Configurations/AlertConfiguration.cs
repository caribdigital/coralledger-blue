using CoralLedger.Blue.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoralLedger.Blue.Infrastructure.Data.Configurations;

public class AlertConfiguration : IEntityTypeConfiguration<Alert>
{
    public void Configure(EntityTypeBuilder<Alert> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.Severity)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(e => e.Message)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(e => e.Location)
            .HasColumnType("geometry(Point, 4326)");

        builder.Property(e => e.Data)
            .HasColumnType("jsonb");

        builder.Property(e => e.AcknowledgedBy)
            .HasMaxLength(200);

        builder.HasOne(e => e.MarineProtectedArea)
            .WithMany()
            .HasForeignKey(e => e.MarineProtectedAreaId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.Vessel)
            .WithMany()
            .HasForeignKey(e => e.VesselId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => e.AlertRuleId);
        builder.HasIndex(e => e.Type);
        builder.HasIndex(e => e.Severity);
        builder.HasIndex(e => e.CreatedAt);
        builder.HasIndex(e => e.IsAcknowledged);
        builder.HasIndex(e => new { e.Type, e.CreatedAt });
    }
}
