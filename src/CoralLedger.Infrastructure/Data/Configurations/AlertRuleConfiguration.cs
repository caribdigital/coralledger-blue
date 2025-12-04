using CoralLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoralLedger.Infrastructure.Data.Configurations;

public class AlertRuleConfiguration : IEntityTypeConfiguration<AlertRule>
{
    public void Configure(EntityTypeBuilder<AlertRule> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.Severity)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(e => e.Conditions)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(e => e.NotificationChannels)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.NotificationEmails)
            .HasMaxLength(500);

        builder.HasOne(e => e.MarineProtectedArea)
            .WithMany()
            .HasForeignKey(e => e.MarineProtectedAreaId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(e => e.Alerts)
            .WithOne(a => a.AlertRule)
            .HasForeignKey(a => a.AlertRuleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.Type);
        builder.HasIndex(e => e.IsActive);
        builder.HasIndex(e => e.MarineProtectedAreaId);
    }
}
