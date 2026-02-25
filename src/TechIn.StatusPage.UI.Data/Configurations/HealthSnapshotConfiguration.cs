using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechIn.StatusPage.Core.Models;

namespace TechIn.StatusPage.UI.Data.Configurations;

public sealed class HealthSnapshotConfiguration : IEntityTypeConfiguration<HealthSnapshot>
{
    public void Configure(EntityTypeBuilder<HealthSnapshot> builder)
    {
        builder.ToTable("snapshots");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.ServiceName)
            .HasColumnName("service_name")
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(x => x.Timestamp)
            .HasColumnName("timestamp")
            .IsRequired();

        builder.Property(x => x.Latency)
            .HasColumnName("latency")
            .IsRequired(false);

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .IsRequired(false)
            .HasMaxLength(2048);

        // Indexes
        builder.HasIndex(x => x.ServiceName).HasDatabaseName("ix_snapshots_service_name");
        builder.HasIndex(x => new { x.ServiceName, x.Timestamp }).HasDatabaseName("ix_snapshots_service_name_timestamp");
        builder.HasIndex(x => x.Timestamp).HasDatabaseName("ix_snapshots_timestamp");
    }
}