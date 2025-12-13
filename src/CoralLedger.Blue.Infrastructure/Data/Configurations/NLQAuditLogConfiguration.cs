using CoralLedger.Blue.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoralLedger.Blue.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for NLQ audit logging
/// Sprint 5.1: Captures all NLQ queries for security and debugging
/// </summary>
public class NLQAuditLogConfiguration : IEntityTypeConfiguration<NLQAuditLog>
{
    public void Configure(EntityTypeBuilder<NLQAuditLog> builder)
    {
        builder.ToTable("nlq_audit_logs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OriginalQuery)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.InterpretedAs)
            .HasMaxLength(1000);

        builder.Property(x => x.GeneratedSql)
            .HasMaxLength(4000);

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(1000);

        builder.Property(x => x.DataSourcesUsed)
            .HasMaxLength(500);

        builder.Property(x => x.UserIp)
            .HasMaxLength(45);

        builder.Property(x => x.UserAgent)
            .HasMaxLength(500);

        builder.Property(x => x.QueryTime)
            .IsRequired();

        // Store enums as strings for readability
        builder.Property(x => x.Persona)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        // Indexes for querying audit logs
        builder.HasIndex(x => x.QueryTime);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.Persona);
        builder.HasIndex(x => x.SecurityRestrictionApplied);
    }
}
